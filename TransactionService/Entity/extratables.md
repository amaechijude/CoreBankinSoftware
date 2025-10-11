Table: transaction_holds
Fields:
- id (BIGINT, Primary Key, Auto-increment)
- hold_reference (VARCHAR(50), Unique, Indexed)
- transaction_reference (VARCHAR(50), Indexed)
- account_number (VARCHAR(10), Indexed)
- hold_amount (DECIMAL(19,2))
- hold_type (VARCHAR(20))
  // "TRANSACTION", "LIEN", "PND" (Post-No-Debit)
  
- status (VARCHAR(20))
  // "ACTIVE", "RELEASED", "EXPIRED"
  
- placed_at (TIMESTAMP)
- released_at (TIMESTAMP, Nullable)
- expires_at (TIMESTAMP)
  // Auto-release after certain time
  
- placed_by_service (VARCHAR(50))
- created_at (TIMESTAMP)

Indexes:
- PRIMARY KEY (id)
- UNIQUE INDEX (hold_reference)
- INDEX (account_number, status)
- INDEX (transaction_reference)
- INDEX (expires_at, status) // For cleanup jobs

Table: transaction_notifications
Fields:
- id (BIGINT, Primary Key, Auto-increment)
- transaction_reference (VARCHAR(50), Indexed)
- recipient_type (VARCHAR(20))
  // "SENDER", "RECEIVER"
  
- notification_type (VARCHAR(20))
  // "SMS", "EMAIL", "PUSH", "IN_APP"
  
- recipient_contact (VARCHAR(100))
  // Phone number or email
  
- message_content (TEXT)
- status (VARCHAR(20))
  // "PENDING", "SENT", "DELIVERED", "FAILED"
  
- provider_reference (VARCHAR(100), Nullable)
  // SMS/Email provider reference
  
- provider_response (TEXT, Nullable)
- retry_count (INT, Default: 0)
- sent_at (TIMESTAMP, Nullable)
- delivered_at (TIMESTAMP, Nullable)
- created_at (TIMESTAMP)

Indexes:
- PRIMARY KEY (id)
- INDEX (transaction_reference)
- INDEX (status, created_at)

Table: transaction_disputes
Fields:
- id (BIGINT, Primary Key, Auto-increment)
- dispute_reference (VARCHAR(50), Unique)
- transaction_reference (VARCHAR(50), Indexed)
- transaction_id (BIGINT, Foreign Key)
- dispute_type (VARCHAR(30))
  // "UNAUTHORIZED", "AMOUNT_MISMATCH", "NOT_RECEIVED", "DUPLICATE"
  
- raised_by_account (VARCHAR(10))
- dispute_description (TEXT)
- status (VARCHAR(20))
  // "OPEN", "INVESTIGATING", "RESOLVED", "REJECTED"
  
- resolution (TEXT, Nullable)
- resolved_by_user_id (VARCHAR(50), Nullable)
- raised_at (TIMESTAMP)
- resolved_at (TIMESTAMP, Nullable)
- created_at (TIMESTAMP)
- updated_at (TIMESTAMP)

Indexes:
- PRIMARY KEY (id)
- UNIQUE INDEX (dispute_reference)
- INDEX (transaction_reference)
- INDEX (status, raised_at)