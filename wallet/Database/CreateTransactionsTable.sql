CREATE TABLE Transactions (
    TransactionID   INT PRIMARY KEY IDENTITY(1,1),
    BuyerID         INT NOT NULL,
    SellerID        INT NULL,
    ItemID          INT NOT NULL,
    Amount          DECIMAL(10,2) NOT NULL,
    Type            NVARCHAR(50) NOT NULL,   -- 'MoviePurchase', 'TicketPurchase', 'EquipmentPurchase', 'EquipmentSale' , 'TopUp'
    Status          NVARCHAR(50) NOT NULL,   -- 'Pending', 'Completed', 'Failed'
    Timestamp       DATETIME DEFAULT GETDATE(),

    CONSTRAINT CHK_Amount CHECK (Amount <> 0),
    CONSTRAINT CHK_Type CHECK (Type IN ('MoviePurchase', 'TicketPurchase', 'EquipmentPurchase','EquipmentSale', 'TopUp')),
    CONSTRAINT CHK_Status CHECK (Status IN ('Pending', 'Completed', 'Failed'))
);