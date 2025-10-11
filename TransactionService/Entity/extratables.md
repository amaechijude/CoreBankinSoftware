Table: transaction_holds
Fields:
- id (GUID, Primary Key)
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
- id (GUID, Primary Key)
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
- id (GUID, Primary Key)
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

---

Table: transaction_reversals
Fields:
- id (GUID, Primary Key)
- reversal_reference (VARCHAR(50), Unique)
- original_transaction_reference (VARCHAR(50), Indexed, Foreign Key to `transactions`)
- reversal_transaction_reference (VARCHAR(50), Indexed, Foreign Key to a new transaction record for the reversal itself)
- reason (TEXT) // "Duplicate transaction", "Customer request", "Resolved dispute"
- status (VARCHAR(20)) // "PENDING", "SUCCESSFUL", "FAILED"
- initiated_by (VARCHAR(50)) // e.g., "SYSTEM", "ADMIN_USER_ID", "DISPUTE_SERVICE"
- initiated_at (TIMESTAMP)
- completed_at (TIMESTAMP, Nullable)

---

Table: transaction_fee_breakdown
Fields:
- id (GUID, Primary Key)
- transaction_reference (VARCHAR(50), Indexed, Foreign Key to `transactions`)
- fee_type (VARCHAR(30)) // "COMMISSION", "SWITCH_FEE", "VAT", "STAMP_DUTY"
- description (VARCHAR(255)) // "NIP transfer commission"
- amount (DECIMAL(19,2))
- created_at (TIMESTAMP)

---

Table: transaction_status_log
Fields:
- id (GUID, Primary Key)
- transaction_reference (VARCHAR(50), Indexed, Foreign Key to `transactions`)
- previous_status (VARCHAR(20), Nullable)
- new_status (VARCHAR(20)) // "INITIATED", "PENDING", "SUCCESSFUL", "FAILED"
- change_reason (VARCHAR(255)) // "Processor confirmation received", "NIBSS timeout"
- metadata (TEXT, Nullable) // Optional JSON blob with extra context, like an API response
- created_at (TIMESTAMP)

---

Table: recurring_transaction_schedules
Fields:
- id (GUID, Primary Key)
- schedule_reference (VARCHAR(50), Unique)
- customer_id (GUID)
- source_account_number (VARCHAR(10))
- destination_details (JSON or separate columns for account, bank, etc.)
- amount (DECIMAL(19,2))
- frequency (VARCHAR(20)) // "DAILY", "WEEKLY", "MONTHLY", "QUARTERLY"
- start_date (DATE)
- end_date (DATE, Nullable)
- next_run_date (DATE, Indexed)
- status (VARCHAR(20)) // "ACTIVE", "PAUSED", "COMPLETED", "CANCELLED"
- created_at (TIMESTAMP)