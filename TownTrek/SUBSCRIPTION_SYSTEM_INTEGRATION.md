# TownTrek Subscription Authentication System

## Overview

This document outlines the comprehensive subscription-based authentication system implemented for TownTrek, which ensures users have appropriate access based on their subscription tier and payment status.

## System Components

### 1. Core Services

#### ISubscriptionAuthService
- **Purpose**: Central service for validating user subscription status and permissions
- **Key Methods**:
  - `ValidateUserSubscriptionAsync(userId)`: Complete subscription validation
  - `HasActiveSubscriptionAsync(userId)`: Quick subscription check
  - `IsPaymentValidAsync(userId)`: Payment status validation
  - `GetUserLimitsAsync(userId)`: Retrieve user's current limits and usage
  - `CanAccessFeatureAsync(userId, featureKey)`: Feature-specific access control

#### SubscriptionAuthResult
- **Purpose**: Standardized response object for subscription validation
- **Properties**: Authentication status, subscription status, payment validity, redirect URLs, error messages

### 2. Authorization Components

#### RequireActiveSubscriptionAttribute
- **Purpose**: Custom authorization attribute for controller actions
- **Features**:
  - Enforces subscription requirements
  - Supports feature-specific access control
  - Allows free tier access when specified
  - Automatic redirects based on user status

#### SubscriptionRedirectMiddleware
- **Purpose**: Handles automatic redirects for payment issues
- **Functionality**:
  - Intercepts requests to client routes
  - Redirects users with invalid payment status
  - Preserves user experience during payment flows

### 3. Database Integration

#### Subscription Tiers
- **Basic Plan**: R199/month - 1 business, 5 images, basic support
- **Standard Plan**: R399/month - 3 businesses, 15 images, priority support, analytics, PDFs
- **Premium Plan**: R599/month - 10 businesses, unlimited images/PDFs, all features

#### Payment Status Validation
- **Valid Statuses**: "Completed", "Active", "Paid"
- **Invalid Statuses**: "Pending", "Rejected", "Failed"

## Implementation Details

### 1. User Authentication Flow

```csharp
// Login process with subscription validation
var authResult = await _subscriptionAuthService.ValidateUserSubscriptionAsync(userId);

if (!authResult.IsPaymentValid && authResult.HasActiveSubscription)
{
    // Redirect to payment processing
    return Redirect(authResult.RedirectUrl);
}
```

### 2. Controller Protection

```csharp
// Require active subscription with free tier fallback
[RequireActiveSubscription(allowFreeTier: true)]
public async Task<IActionResult> Dashboard() { }

// Require specific feature access
[RequireActiveSubscription(requiredFeature: "BasicAnalytics")]
public async Task<IActionResult> Analytics() { }
```

### 3. View Integration

```razor
@* Display subscription-specific information *@
@if (Model.UserLimits != null)
{
    <div class="stat-change">
        @Model.UserLimits.CurrentBusinessCount/@(Model.UserLimits.MaxBusinesses == -1 ? "∞" : Model.UserLimits.MaxBusinesses.ToString()) used
    </div>
}
```

## User Experience Scenarios

### Scenario 1: User with Standard Plan and Pending Payment
1. **Login**: User logs in successfully
2. **Validation**: System detects pending payment status
3. **Dashboard Access**: User can view dashboard with warning message
4. **Feature Restrictions**: Advanced features show upgrade prompts
5. **Payment Redirect**: System provides payment completion link

### Scenario 2: User with Basic Plan Accessing Premium Features
1. **Feature Request**: User tries to access Analytics
2. **Authorization Check**: System validates feature access
3. **Redirect**: User redirected to subscription upgrade page
4. **Upgrade Prompt**: Clear messaging about required plan level

### Scenario 3: Free Tier User
1. **Limited Access**: Can access basic dashboard functionality
2. **Usage Limits**: 1 business, 3 images maximum
3. **Upgrade Prompts**: Contextual upgrade suggestions
4. **Feature Restrictions**: Premium features disabled with upgrade prompts

## Configuration

### Service Registration (Program.cs)
```csharp
builder.Services.AddScoped<ISubscriptionAuthService, SubscriptionAuthService>();
builder.Services.AddScoped<IRoleInitializationService, RoleInitializationService>();
```

### Middleware Registration
```csharp
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<TownTrek.Middleware.SubscriptionRedirectMiddleware>();
```

### Role Initialization
The system automatically creates the following roles:
- Admin
- Member
- Client-Basic
- Client-Standard
- Client-Premium

## Testing Endpoints

### Development Testing
- `/Test/CheckSubscription` - Validate user subscription status
- `/Test/GetUserLimits` - Check current usage limits
- `/Test/CheckFeature?featureKey=BasicAnalytics` - Test feature access

## Security Considerations

### Payment Data Protection
- No sensitive payment data stored in application database
- Only safe payment references (tokens, IDs) are maintained
- Payment processing handled by external providers (PayFast)

### Access Control
- Multi-layered authorization (middleware, attributes, service-level)
- Graceful degradation for expired subscriptions
- Secure redirect handling for payment flows

## Monitoring and Logging

### Key Log Events
- Subscription validation failures
- Payment status changes
- Feature access denials
- Automatic redirects

### Performance Considerations
- Efficient database queries with proper includes
- Caching of subscription data where appropriate
- Minimal middleware overhead

## Future Enhancements

### Planned Features
1. **Subscription Analytics**: Track usage patterns and limits
2. **Automated Notifications**: Email alerts for payment issues
3. **Grace Periods**: Temporary access during payment processing
4. **Usage Warnings**: Proactive limit notifications

### Integration Points
- PayFast payment gateway integration
- Email notification system
- Analytics and reporting dashboard
- Mobile app API endpoints

## Troubleshooting

### Common Issues
1. **Subscription Not Found**: Check user has active subscription record
2. **Payment Status Issues**: Verify PayFast webhook processing
3. **Feature Access Denied**: Confirm subscription tier includes required features
4. **Redirect Loops**: Check middleware configuration and route patterns

### Debug Information
The system provides comprehensive logging for troubleshooting subscription-related issues. Check application logs for detailed error messages and validation results.

## Conclusion

This subscription authentication system provides a robust, scalable foundation for managing user access based on subscription tiers and payment status. It ensures proper access control while maintaining a smooth user experience and clear upgrade paths.