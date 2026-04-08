using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BoardRent.Data;
using BoardRent.DataTransferObjects;
using BoardRent.Repositories;
using BoardRent.Utils;

namespace BoardRent.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        // Named constants to eliminate magic numbers
        private const int MinimumDisplayNameLength = 2;
        private const int MaximumDisplayNameLength = 50;
        private const int MaximumStreetNumberLength = 10;
        private const string StandardUserRoleName = "Standard User";
        private const string AvatarFolderName = "Avatars";
        private const string ApplicationName = "BoardRent";

        public UserService(IUserRepository userRepository, IUnitOfWorkFactory unitOfWorkFactory)
        {
            _userRepository = userRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public async Task<ServiceResult<UserProfileDataTransferObject>> GetProfileAsync(Guid userId)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                await unitOfWork.OpenAsync();
                _userRepository.SetUnitOfWork(unitOfWork);

                var userEntity = await _userRepository.GetByIdAsync(userId);
                if (userEntity == null)
                {
                    return ServiceResult<UserProfileDataTransferObject>.Fail("User not found.");
                }

                return ServiceResult<UserProfileDataTransferObject>.Ok(MapEntityToProfileDataTransferObject(userEntity));
            }
        }

        public async Task<ServiceResult<bool>> UpdateProfileAsync(Guid userId, UserProfileDataTransferObject profileUpdateData)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                await unitOfWork.OpenAsync();
                _userRepository.SetUnitOfWork(unitOfWork);

                var userEntity = await _userRepository.GetByIdAsync(userId);
                if (userEntity == null)
                {
                    return ServiceResult<bool>.Fail("User not found.");
                }

                var validationErrors = ValidateProfileDetails(profileUpdateData);

                // Business Logic: Verify email uniqueness if it has been changed
                if (!string.IsNullOrWhiteSpace(profileUpdateData.Email) && profileUpdateData.Email != userEntity.Email)
                {
                    var userWithDuplicateEmail = await _userRepository.GetByEmailAsync(profileUpdateData.Email);
                    if (userWithDuplicateEmail != null && userWithDuplicateEmail.Id != userId)
                    {
                        validationErrors.Add("Email|This email address is already taken by another account.");
                    }
                }

                if (validationErrors.Any())
                {
                    return ServiceResult<bool>.Fail(string.Join(";", validationErrors));
                }

                ApplyProfileUpdatesToEntity(userEntity, profileUpdateData);
                await _userRepository.UpdateAsync(userEntity);

                return ServiceResult<bool>.Ok(true);
            }
        }

        public async Task<ServiceResult<bool>> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                await unitOfWork.OpenAsync();
                _userRepository.SetUnitOfWork(unitOfWork);

                var userEntity = await _userRepository.GetByIdAsync(userId);
                if (userEntity == null)
                {
                    return ServiceResult<bool>.Fail("User not found.");
                }

                if (!PasswordHasher.VerifyPassword(currentPassword, userEntity.PasswordHash))
                {
                    return ServiceResult<bool>.Fail("Current password is incorrect.");
                }

                var (isPasswordValid, passwordErrorMessage) = PasswordValidator.Validate(newPassword);
                if (!isPasswordValid)
                {
                    return ServiceResult<bool>.Fail(passwordErrorMessage);
                }

                userEntity.PasswordHash = PasswordHasher.HashPassword(newPassword);
                userEntity.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(userEntity);

                // Clear session context to force re-authentication as per requirement UM-S04
                SessionContext.GetInstance().Clear();

                return ServiceResult<bool>.Ok(true);
            }
        }

        public async Task<string> UploadAvatarAsync(Guid userId, string sourceFilePath)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                await unitOfWork.OpenAsync();
                _userRepository.SetUnitOfWork(unitOfWork);

                var userEntity = await _userRepository.GetByIdAsync(userId);
                if (userEntity == null)
                {
                    throw new Exception("User not found.");
                }

                string fileName = $"{userId}_{Path.GetFileName(sourceFilePath)}";
                string localApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string saveFolderPath = Path.Combine(localApplicationData, ApplicationName, AvatarFolderName);

                Directory.CreateDirectory(saveFolderPath);
                string destinationPath = Path.Combine(saveFolderPath, fileName);

                File.Copy(sourceFilePath, destinationPath, true);

                userEntity.AvatarUrl = destinationPath;
                userEntity.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(userEntity);

                return destinationPath;
            }
        }

        public async Task RemoveAvatarAsync(Guid userId)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                await unitOfWork.OpenAsync();
                _userRepository.SetUnitOfWork(unitOfWork);

                var userEntity = await _userRepository.GetByIdAsync(userId);
                if (userEntity == null)
                {
                    throw new Exception("User not found.");
                }

                userEntity.AvatarUrl = null;
                userEntity.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(userEntity);
            }
        }

        private List<string> ValidateProfileDetails(UserProfileDataTransferObject profileData)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(profileData.DisplayName) ||
                profileData.DisplayName.Length < MinimumDisplayNameLength ||
                profileData.DisplayName.Length > MaximumDisplayNameLength)
            {
                errors.Add("DisplayName|Display name must be between 2 and 50 characters long.");
            }

            if (!string.IsNullOrWhiteSpace(profileData.PhoneNumber))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(profileData.PhoneNumber, @"^\+?\d{7,15}$"))
                {
                    errors.Add("PhoneNumber|Phone number format is invalid.");
                }
            }

            if (!string.IsNullOrWhiteSpace(profileData.StreetNumber) && profileData.StreetNumber.Length > MaximumStreetNumberLength)
            {
                errors.Add("StreetNumber|Street number must be a valid value.");
            }

            return errors;
        }

        private void ApplyProfileUpdatesToEntity(Domain.User userEntity, UserProfileDataTransferObject profileUpdateData)
        {
            userEntity.DisplayName = profileUpdateData.DisplayName;
            userEntity.Email = profileUpdateData.Email;
            userEntity.PhoneNumber = profileUpdateData.PhoneNumber;
            userEntity.Country = profileUpdateData.Country;
            userEntity.City = profileUpdateData.City;
            userEntity.StreetName = profileUpdateData.StreetName;
            userEntity.StreetNumber = profileUpdateData.StreetNumber;
            userEntity.UpdatedAt = DateTime.UtcNow;
        }

        private UserProfileDataTransferObject MapEntityToProfileDataTransferObject(Domain.User userEntity)
        {
            var primaryRole = userEntity.Roles?.FirstOrDefault();

            return new UserProfileDataTransferObject
            {
                Id = userEntity.Id,
                Username = userEntity.Username,
                DisplayName = userEntity.DisplayName,
                Email = userEntity.Email,
                PhoneNumber = userEntity.PhoneNumber,
                AvatarUrl = userEntity.AvatarUrl,
                Role = new RoleDataTransferObject
                {
                    Id = primaryRole?.Id ?? Guid.Empty,
                    Name = primaryRole?.Name ?? StandardUserRoleName
                },
                IsSuspended = userEntity.IsSuspended,
                Country = userEntity.Country,
                City = userEntity.City,
                StreetName = userEntity.StreetName,
                StreetNumber = userEntity.StreetNumber
            };
        }
    }
}