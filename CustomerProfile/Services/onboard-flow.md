## How OPay Registers New Users (General Steps)

While the exact internal process for OPay might have specific nuances, the general steps for a user to register on a mobile money or fintech platform like OPay in Nigeria typically follow this pattern:

1.  **Download the OPay App:** The user first needs to download the OPay mobile application from their device's app store (Google Play Store for Android or Apple App Store for iOS).
2.  **Open the App & Select "Sign Up" / "Register":** Upon opening the app, there will be clear options to either log in (for existing users) or sign up/register (for new users).
3.  **Enter Phone Number:** The user will be prompted to enter their active Nigerian mobile phone number. This number usually serves as their primary identifier and account number.
4.  **OTP Verification:** An SMS containing a One-Time Password (OTP) will be sent to the entered phone number. The user must input this OTP into the app to verify ownership of the phone number.
    *   *Technical Note:* To link the verification to the user, the server temporarily stores the OTP mapped to the phone number (or a returned `reference_id`) in a cache (e.g., Redis). The app sends back this identifier with the OTP for validation.
5.  **Set Up PIN/Password:** The user will then be asked to create a secure 4-digit PIN (for transactions) and/or a password (for app login).
    *   *Technical Note:* Upon submission, the backend checks the `VerificationCode` table to ensure the phone number is verified (`CanSetProfile` is true). It then creates a permanent `User` record in the database, linking the **Phone Number** to the **Hashed Password/PIN**.
6.  **Provide Personal Details (Basic KYC - Tier 1):** To activate the basic account, the user typically needs to provide some personal information, which might include:
    *   Full Name
    *   Date of Birth
    *   Gender
    *   Referral Code (optional)
7.  **Agree to Terms & Conditions:** The user must review and accept OPay's Terms of Service and Privacy Policy.
8.  **Account Creation Confirmation:** Once all steps are completed, the user's basic OPay account is created, and they can start using basic services (e.g., airtime purchase, bill payments, receiving money).

### Further Verification (Tier 2 & 3 - for higher transaction limits):

To unlock higher transaction limits and access more advanced features (like sending larger amounts, linking bank accounts, or applying for loans), users will typically need to complete additional Know Your Customer (KYC) verification steps, which may include:

*   **Providing a valid ID:** This could be a National ID Card (NIN Slip/Card), Driver's License, International Passport, or Voter's Card.
*   **Taking a selfie/live photo:** For facial recognition and identity verification.
*   **Linking a Bank Verification Number (BVN):** This is a crucial step for financial transactions in Nigeria, linking the OPay account to the user's broader financial identity.
*   **Proof of Address:** Utility bill or other documents.

These additional steps align with regulatory requirements from the Central Bank of Nigeria (CBN) for different tiers of mobile money accounts.