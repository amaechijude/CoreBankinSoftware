## Next to do
- ### Core BankingTransaction Services
        1. Client Request
        POST /api/transactions/credit
        {
            "accountNumber": "222200000001234S",
            "amount": 1000.00,
            "narration": "Salary payment"
        }

        2. API Gateway (YARP)
        ├─ Validate JWT token
        ├─ Check user permissions
        └─ Route to Transaction Service

        3. Transaction Service
        ├─ Validate request
        ├─ Call Account Service (gRPC) - Validate account exists
        ├─ Generate SessionID/Reference
        ├─ Build XML request (FTSingleDebitRequest)
        ├─ Call Core Banking API
        ├─ Parse XML response
        ├─ Call Account Service (gRPC) - Update balance
        ├─ Save transaction record
        └─ Return response to client

        4. Account Service (gRPC)
        ├─ Validate account
        ├─ Update balance
        └─ Return updated account info