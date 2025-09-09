# Customer Profile Service Flow Documentation

## Overview
This document outlines the service flow for customer onboarding, authentication, and verification processes in the Customer Profile API.

## Authentication Flow

### 1. Initial Onboarding
- **Entry Point**: User initiates onboarding with phone number and email
- **Validation**: System checks if phone number is already registered
- **Process**: 
  - If new user: Generates OTP and sends SMS
  - If existing user: Notifies existing user of signup attempt via SMS

### 2. Profile Setup
- **Prerequisites**: Valid OTP verification
- **Process**:
  - User provides username and password
  - System creates new UserProfile
  - JWT token generated for authentication
  - Verification code is deleted after successful profile creation

## BVN Verification Flow

### 1. BVN Search
- **Entry Point**: `SearchBvnAsync` in NinBvnService
- **Prerequisites**: 
  - Authenticated user
  - Valid BVN number
- **Process**:
  - Validates BVN search request
  - Calls QuickVerify service to verify BVN
  - Updates user profile with BVN data
  - Stores BVN image for future verification

### 2. Face Verification
- **Entry Point**: `FaceVerificationAsync` in NinBvnService
- **Prerequisites**:
  - Authenticated user
  - Completed BVN search
  - BVN image available
- **Process**:
  - Validates face verification request
  - Compares provided image with stored BVN image
  - Uses FaceRecognition service for comparison
  - Returns similarity score and verification result

## Face Recognition Details

### Comparison Process
- Uses FaceAiSharp for face detection and comparison
- Similarity thresholds:
  - ? 0.42: Same person
  - > 0.28: Uncertain - might be same person
  - < 0.28: Different person

### Security Features
- Face detection ensures single face per image
- Face alignment using landmarks for accurate comparison
- Confidence scores provided for both images

## Security and Validation

### Data Protection
- Passwords are hashed using ASP.NET Identity's PasswordHasher
- JWT tokens used for session management
- Secure storage of biometric data

### Input Validation
- Fluent Validation used throughout the service
- Validation for:
  - Onboarding requests
  - BVN search
  - Face verification
  - Profile setup

## Error Handling
- Comprehensive error messages for all operations
- Validation errors return detailed feedback
- Service unavailability handled gracefully
- Authentication/authorization failures properly managed

## Database Operations
- PostgreSQL database using Entity Framework Core
- Transactional integrity maintained
- Proper user profile management
- Secure storage of verification data