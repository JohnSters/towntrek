using System.ComponentModel.DataAnnotations;
using TownTrek.Models;

namespace TownTrek.Models.ViewModels
{
    public class MemberDashboardViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public List<Town> AvailableTowns { get; set; } = new List<Town>();
        public Town? SelectedTown { get; set; }
        public List<BusinessCardViewModel> FeaturedBusinesses { get; set; } = new List<BusinessCardViewModel>();
        public List<BusinessCardViewModel> RecentBusinesses { get; set; } = new List<BusinessCardViewModel>();
        public int TotalBusinessCount { get; set; }
        public List<FavoriteBusiness> UserFavorites { get; set; } = new List<FavoriteBusiness>();
    }

    public class TownBusinessListViewModel
    {
        public Town Town { get; set; } = null!;
        public List<BusinessCategory> Categories { get; set; } = new List<BusinessCategory>();
        public List<BusinessCardViewModel> Businesses { get; set; } = new List<BusinessCardViewModel>();
        public string? SelectedCategory { get; set; }
        public string? SelectedSubCategory { get; set; }
        public string? SearchTerm { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalResults { get; set; }
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }

    public class BusinessCardViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? SubCategory { get; set; }
        public string? ShortDescription { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string? EmailAddress { get; set; }
        public string? Website { get; set; }
        public string PhysicalAddress { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string? CoverImageUrl { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsVerified { get; set; }
        public decimal? Rating { get; set; }
        public int TotalReviews { get; set; }
        public bool IsUserFavorite { get; set; }
        public string TownName { get; set; } = string.Empty;
        public List<BusinessImage> GalleryImages { get; set; } = new List<BusinessImage>();
        public List<BusinessHour> BusinessHours { get; set; } = new List<BusinessHour>();
        public List<BusinessService> Services { get; set; } = new List<BusinessService>();
    }

    public class BusinessDetailsViewModel
    {
        public BusinessCardViewModel Business { get; set; } = null!;
        public List<ReviewWithResponseViewModel> Reviews { get; set; } = new List<ReviewWithResponseViewModel>();
        public bool CanUserReview { get; set; } = true;
        public BusinessReview? UserReview { get; set; }
        public AddReviewViewModel NewReview { get; set; } = new AddReviewViewModel();
        public List<BusinessCardViewModel> RelatedBusinesses { get; set; } = new List<BusinessCardViewModel>();
    }

    public class AddReviewViewModel
    {
        public int BusinessId { get; set; }
        public int Rating { get; set; } = 5;
        public string? Comment { get; set; }
    }

    public class ReviewSubmissionResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public BusinessReview? Review { get; set; }
    }

    public class ReviewWithResponseViewModel
    {
        public BusinessReview Review { get; set; } = null!;
        public BusinessReviewResponse? Response { get; set; }
        public bool CanUserRespond { get; set; } = false;
    }

    public class AddReviewResponseViewModel
    {
        [Required]
        public int ReviewId { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "Response cannot exceed 1000 characters")]
        public string Response { get; set; } = string.Empty;
    }

    public class ReviewResponseSubmissionResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public BusinessReviewResponse? Response { get; set; }
    }

    public class BusinessSearchViewModel
    {
        public string? SearchTerm { get; set; }
        public int? TownId { get; set; }
        public string? Category { get; set; }
        public string? SubCategory { get; set; }
        public List<Town> AvailableTowns { get; set; } = new List<Town>();
        public List<BusinessCategory> AvailableCategories { get; set; } = new List<BusinessCategory>();
        public List<BusinessCardViewModel> Results { get; set; } = new List<BusinessCardViewModel>();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalResults { get; set; }
        public bool HasResults => Results.Any();
    }
}
