# Review Responses Feature

## Overview
The Review Responses feature allows business owners to respond to customer reviews on their business listings. This creates a dialogue between businesses and customers, improving engagement and customer service.

## Key Features

### 1. Business Owner Responses
- Only the business owner can respond to reviews for their business
- One response per review (business owners cannot edit or delete responses once posted)
- Responses are limited to 1000 characters
- Responses are displayed with a distinctive "Owner" badge

### 2. Visual Design
- Follows the TownTrek design system with consistent styling
- Owner responses are displayed in a light gray container with a blue left border
- Owner badge includes initials in a circular avatar and a yellow "Owner" label
- Responsive design that works on mobile and desktop

### 3. User Experience
- Smooth animations for showing/hiding response forms
- Character counter for response text
- Loading states during submission
- Success/error notifications
- Form validation

## Technical Implementation

### Database Schema
- New `BusinessReviewResponses` table with the following structure:
  - `Id` (Primary Key)
  - `BusinessReviewId` (Foreign Key to BusinessReviews)
  - `UserId` (Foreign Key to AspNetUsers - the business owner)
  - `Response` (Text, max 1000 characters)
  - `IsActive` (Boolean)
  - `CreatedAt` (DateTime)
  - `UpdatedAt` (DateTime, nullable)

### API Endpoints
- `POST /Public/AddReviewResponse` - Submit a new review response

### Models Added
- `BusinessReviewResponse` - Entity model
- `ReviewWithResponseViewModel` - View model combining review and response
- `AddReviewResponseViewModel` - Form model for submitting responses
- `ReviewResponseSubmissionResult` - Result model for API responses

### Services
- Added `SubmitReviewResponseAsync` method to `MemberService`
- Updated `GetBusinessDetailsAsync` to load reviews with responses

### Frontend
- CSS styles in `wwwroot/css/components/review-responses.css`
- JavaScript functionality in `PublicManager` class
- Partial view `_ReviewWithResponse.cshtml` for clean code organization

## Security & Validation

### Authorization
- Only authenticated users can submit responses
- Only business owners can respond to reviews for their businesses
- One response per review limit enforced at database level

### Validation
- Response text is required and limited to 1000 characters
- Server-side validation with proper error handling
- Client-side character counting and validation

### Data Integrity
- Foreign key constraints ensure data consistency
- Soft delete pattern with `IsActive` flag
- Audit trail with creation timestamps

## Usage Instructions

### For Business Owners
1. Navigate to your business listing
2. Scroll to the reviews section
3. Click "Respond" button under any review
4. Type your response (max 1000 characters)
5. Click "Post Response" to submit

### For Customers
- Customer reviews now show business owner responses below the original review
- Owner responses are clearly marked with an "Owner" badge
- Responses show the date they were posted

## Design System Compliance

The feature follows the TownTrek design system:
- Uses design system colors (charcoal, lapis-lazuli, carolina-blue, hunyadi-yellow)
- Consistent spacing using CSS custom properties
- Proper typography scale and font weights
- Responsive design patterns
- Accessibility considerations (WCAG 2.1 AA compliance)

## Future Enhancements

Potential future improvements:
- Edit/delete responses (with audit trail)
- Email notifications when responses are posted
- Response moderation system
- Analytics for response engagement
- Bulk response management for business owners with multiple locations

## Testing

To test the feature:
1. Create a business listing as a business owner
2. Create a customer account and leave a review
3. Log in as the business owner and respond to the review
4. Verify the response appears correctly with proper styling
5. Test responsive design on mobile devices

## Files Modified/Added

### New Files
- `Models/BusinessReviewResponse.cs`
- `Views/Shared/_ReviewWithResponse.cshtml`
- `wwwroot/css/components/review-responses.css`
- `Migrations/20250815173924_AddBusinessReviewResponses.cs`

### Modified Files
- `Data/ApplicationDbContext.cs` - Added DbSet and configuration
- `Models/ViewModels/MemberViewModels.cs` - Added new view models
- `Services/MemberService.cs` - Added response functionality
- `Services/Interfaces/IMemberService.cs` - Added interface method
- `Controllers/Public/PublicController.cs` - Added API endpoint
- `Views/Public/BusinessDetails.cshtml` - Updated to use new structure
- `wwwroot/css/entrypoints/public.css` - Added CSS import
- `wwwroot/js/modules/public/public-manager.js` - Added JavaScript functionality

This feature enhances the TownTrek platform by enabling better communication between businesses and customers, ultimately improving the overall user experience and business engagement.