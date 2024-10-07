using FluentValidation;
using FluentValidation.Results;
using FluentValidation.Validators;
using Komikai;
using Komikai.Data;
using Komikai.Data.Entities;
using Komikai.Helpers;
using Microsoft.EntityFrameworkCore;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Results;
using SharpGrip.FluentValidation.AutoValidation.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ForumDbContext>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddFluentValidationAutoValidation(configuration =>
{
    configuration.OverrideDefaultResultFactoryWith<ProblemDetailsResultFactory>();
});
var app = builder.Build();

/*
    /api/v1/topics GET List 200
    /api/v1/topics POST Create 201
    /api/v1/topics/{id} GET One 200
    /api/v1/topics/{id} PUT/PATCH Modify 200
    /api/v1/topics/{id} DELETE Remove 200/204
 */

/*
    /api/v1/topics/{topicId}/posts GET List 200
    /api/v1/topics/{topicId}/posts/{postId} GET One 200
    /api/v1/topics/{topicId}/posts POST Create 201
    /api/v1/topics/{topicId}/posts/{postId} PUT/PATCH Modify 200
    /api/v1/topics/{topicId}/posts/{postId} DELETE Remove 200/204
*/


app.AddComedianApi();
app.AddSetsApi();
app.AddCommentsApi();

app.Run();


public record SetDto(int Id, string Title, string Body, DateTimeOffset CreationDate);
public record CreateSetDto(string Title, string Body)
{
    public class CreateSetDtoValidator : AbstractValidator<CreateSetDto>
    {
        public CreateSetDtoValidator()
        {
            RuleFor(x => x.Title).NotEmpty().Length(min: 2, max: 80).Matches(@"^[a-zA-Z0-9 .,;-]+$").WithMessage("Title can only contain letters, numbers, spaces, and the following punctuation: .,;-");
            RuleFor(x => x.Body).NotEmpty().Length(min: 5, max: 300).Matches(@"^[a-zA-Z0-9 .,;-]+$").WithMessage("Body can only contain letters, numbers, spaces, and the following punctuation: .,;-");
        }
    }
}
public record UpdateSetDto(string Body)
{
    public class UpdateSetDtoValidator : AbstractValidator<UpdateSetDto>
    {
        public UpdateSetDtoValidator()
        {
            RuleFor(x => x.Body).NotEmpty().Length(min: 5, max: 300);
        }
    }
};





public record CommentDto(int Id, string Content, DateTimeOffset CreationDate);
public record CreateCommentDto(string Content)
{
    public class CreateCommentDtoValidator : AbstractValidator<CreateCommentDto>
    {
        public CreateCommentDtoValidator()
        {
            RuleFor(x => x.Content).NotEmpty().Length(min: 5, max: 300).Matches(@"^[a-zA-Z0-9 .,;-]+$").WithMessage("Comment can only contain letters, numbers, spaces, and the following punctuation: .,;-"); 
        }
    }
}
public record UpdateCommentDto(string Content)
{
    public class UpdateCommentDtoValidator : AbstractValidator<UpdateCommentDto>
    {
        public UpdateCommentDtoValidator()
        {
            RuleFor(x => x.Content).NotEmpty().Length(min: 5, max: 300).Matches(@"^[a-zA-Z0-9 .,;-]+$").WithMessage("Comment can only contain letters, numbers, spaces, and the following punctuation: .,;-"); 
        }
    }
};





public class ProblemDetailsResultFactory : IFluentValidationAutoValidationResultFactory
{
    public IResult CreateResult(EndpointFilterInvocationContext contex, ValidationResult validationResult)
    {
        var problemDetails = new HttpValidationProblemDetails(validationResult.ToValidationProblemErrors())
        {
            Type = "https://tools.ietf.org/html/rfc4918#section-11.2",
            Title = "Unprocessable Entity",
            Status = 422
        };
         return TypedResults.Problem(problemDetails);
    }
}
public record ComedianDto(int Id, string Name, string Description);

public record CreateComedianDto(string Name, string Description)
{
    public class CreateComedianDtoValidator : AbstractValidator<CreateComedianDto>
    {
        public CreateComedianDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().Length(min:2, max: 80).Matches("^[a-zA-Z ]+$").WithMessage("Name can only contain letters and spaces.");
            RuleFor(x => x.Description).NotEmpty().Length(min: 5, max: 300).Matches(@"^[a-zA-Z0-9 .,;-]+$").WithMessage("Description can only contain letters, numbers, spaces, and the following punctuation: .,;-");
        }
    }
}
public record UpdateComedianDto(string Description) {
    public class UpdateComedianDtoValidator : AbstractValidator<UpdateComedianDto>
    {
        public UpdateComedianDtoValidator()
        {
            RuleFor(x => x.Description).NotEmpty().Length(min: 5, max: 300).Matches(@"^[a-zA-Z0-9 .,;-]+$").WithMessage("Description can only contain letters, numbers, spaces, and the following punctuation: .,;-"); 
        }
    }
};

