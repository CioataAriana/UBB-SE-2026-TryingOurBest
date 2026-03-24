using CommunityToolkit.Mvvm.Input;
using System;
using System.ComponentModel;
using TryingOurBest1.Models;

namespace TryingOurBest1.ViewModels
{
    public class MoviePurchaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private int _currentUserID;

        private decimal _balance;
        public decimal Balance
        {
            get => _balance;
            set { _balance = value; OnPropertyChanged(nameof(Balance)); }
        }

        private decimal _price;
        public decimal Price
        {
            get => _price;
            set { _price = value; OnPropertyChanged(nameof(Price)); }
        }

        private int _movieID;
        public int MovieID
        {
            get => _movieID;
            set { _movieID = value; OnPropertyChanged(nameof(MovieID)); }
        }

        private bool _isOwned;
        public bool IsOwned
        {
            get => _isOwned;
            set { _isOwned = value; OnPropertyChanged(nameof(IsOwned)); }
        }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(nameof(ErrorMessage)); }
        }

        private string _purchaseStatusMessage = string.Empty;
        public string PurchaseStatusMessage
        {
            get => _purchaseStatusMessage;
            set { _purchaseStatusMessage = value; OnPropertyChanged(nameof(PurchaseStatusMessage)); }
        }

        // --- Success Modal ---
        private bool _isSuccessVisible;
        public bool IsSuccessVisible
        {
            get => _isSuccessVisible;
            set { _isSuccessVisible = value; OnPropertyChanged(nameof(IsSuccessVisible)); }
        }

        private string _successModalMessage = string.Empty;
        public string SuccessModalMessage
        {
            get => _successModalMessage;
            set { _successModalMessage = value; OnPropertyChanged(nameof(SuccessModalMessage)); }
        }

        // --- Commands ---
        public IRelayCommand BuyMovieCommand { get; }
        public IRelayCommand DismissSuccessCommand { get; }

        // --- Constructor ---
        public MoviePurchaseViewModel(int userID, decimal userBalance, int movieID, decimal moviePrice, bool alreadyOwned)
        {
            _currentUserID = userID;
            _balance = userBalance;
            _movieID = movieID;
            _price = moviePrice;
            _isOwned = alreadyOwned;

            BuyMovieCommand = new RelayCommand(BuyMovie);
            DismissSuccessCommand = new RelayCommand(DismissSuccess);
        }

        // --- Buy Movie Logic ---
        private void BuyMovie()
        {
            ErrorMessage = string.Empty;
            PurchaseStatusMessage = string.Empty;

            if (IsOwned)
            {
                ErrorMessage = "You already own this movie.";
                return;
            }

            if (Balance >= Price)
            {
                UpdateBalance(-Price);
                AddOwnership(MovieID);
                LogTransaction();

                IsOwned = true;

                // Show success modal
                SuccessModalMessage = "Thank you for your purchase! The movie has been added to your library.";
                IsSuccessVisible = true;
            }
            else
            {
                ErrorMessage = "Insufficient funds. Please top up your wallet.";
            }
        }

        // --- Dismiss Modal ---
        private void DismissSuccess()
        {
            IsSuccessVisible = false;
            SuccessModalMessage = string.Empty;
        }

        // --- Helpers ---
        private void UpdateBalance(decimal amount)
        {
            Balance += amount;
            OnPropertyChanged(nameof(Balance));
            // TODO: call UserRepository.UpdateBalance(_currentUserID, Balance)
        }

        private void AddOwnership(int movieID)
        {
            // TODO: call OwnedMoviesRepository.AddOwnership(_currentUserID, movieID)
        }

        private void LogTransaction()
        {
            var transaction = new Transaction
            {
                BuyerID = _currentUserID,
                SellerID = null,
                ItemID = MovieID,
                Amount = -Price,
                Type = "MoviePurchase",
                Status = "Completed",
                Timestamp = DateTime.Now
            };
            // TODO: call TransactionRepository.Add(transaction)
        }
    }
}