﻿using Komikai.Data.Entities;
using Komikai.Data;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using Microsoft.EntityFrameworkCore;
using Komikai.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Text.Json;
using System.ComponentModel.Design;

namespace Komikai
{
    public static class Endpoints
    {
        /*
        /api/v1/topics GET List 200
        /api/v1/topics POST Create 201
        /api/v1/topics/{id} GET One 200
        /api/v1/topics/{id} PUT/PATCH Modify 200
        /api/v1/topics/{id} DELETE Remove 200/204
        */

       
        public static void AddComedianApi(this  WebApplication app)
        {
            var comediansGroup = app.MapGroup("/api").AddFluentValidationAutoValidation();

            comediansGroup.MapGet("/comedians", async([AsParameters] SearchParameters searchParams, LinkGenerator linkGenerator, HttpContext httpContext, ForumDbContext dbContext) =>
            {

                return(await dbContext.Comedians.ToListAsync()).Select(comedian => comedian.ToDto());

            }).WithName("GetComedians");


            comediansGroup.MapGet("/comedians/{comedianId}", async (int comedianId, ForumDbContext dbContext) =>
            {
                var comedian = await dbContext.Comedians.FindAsync(comedianId);

                if (comedian == null)
                {
                    return Results.NotFound($"No comedian found with ID {comedianId}.");
                }

                return TypedResults.Ok(comedian.ToDto());
            }).WithName("GetComedian");


            comediansGroup.MapPost("/comedians", async (CreateComedianDto dto, ForumDbContext dbContext) => {
                var comedian = new Comedian { Name = dto.Name, Description = dto.Description };
                dbContext.Comedians.Add(comedian);

                await dbContext.SaveChangesAsync();

                return TypedResults.Created($"api/comedians/{comedian.Id}", comedian.ToDto());
            }).WithName("CreateComedian");

            comediansGroup.MapPut("/comedians/{comedianId}", async (UpdateComedianDto dto, int comedianId, ForumDbContext dbContext) =>
            {
                var comedian = await dbContext.Comedians.FindAsync(comedianId);
                if (comedian == null)
                {
                    return Results.NotFound($"No comedian found with ID {comedianId}.");
                }

                comedian.Description = dto.Description;

                dbContext.Comedians.Update(comedian);
                await dbContext.Comedians.ToListAsync();

                return Results.Ok(comedian.ToDto());
            }).WithName("UpdateComedian");

            comediansGroup.MapDelete("/comedians/{comedianId}", async (int comedianId, ForumDbContext dbContext) =>
            {
                var comedian = await dbContext.Comedians.FindAsync(comedianId);
                if (comedian == null)
                {
                    return Results.NotFound($"Comedian with ID {comedianId} not found");
                }
                dbContext.Comedians.Remove(comedian);
                await dbContext.SaveChangesAsync();
                return Results.NoContent();
            }).WithName("RemoveComedian");
        }

        /*
            /api/v1/topics/{topicId}/posts GET List 200
            /api/v1/topics/{topicId}/posts/{postId} GET One 200
            /api/v1/topics/{topicId}/posts POST Create 201
            /api/v1/topics/{topicId}/posts/{postId} PUT/PATCH Modify 200
            /api/v1/topics/{topicId}/posts/{postId} DELETE Remove 200/204
        */
        public static void AddSetsApi(this WebApplication app)
        {
            var setsGroup = app.MapGroup("/api/comedians/{comedianId}").AddFluentValidationAutoValidation();

            setsGroup.MapGet("sets", async (int comedianId, ForumDbContext dbContext, CancellationToken cancellationToken) =>
            {

                var sets = await dbContext.Sets
                    .Where(set => set.Comedian.Id == comedianId)
                    .ToListAsync(cancellationToken);

                if (!sets.Any())
                {
                    return Results.NotFound($"No sets found for comedian with ID {comedianId}.");
                }
                return Results.Ok(sets.Select(set => set.ToDto()));

            });

           /* setsGroup.MapGet("sets", async (int comedianId, [AsParameters] SearchParameters searchParams, LinkGenerator linkGenerator, HttpContext httpContext, ForumDbContext dbContext, CancellationToken cancellationToken) =>
            {

                var queryable = dbContext.Sets.Where(set => set.Comedian.Id == comedianId).AsQueryable().OrderBy(o => o.Title);

                if (!queryable.Any())
                {
                    return Results.NotFound($"No sets found for comedian with ID {comedianId}.");
                }

                var pagedList = await PagedList<Set>.CreateAsync(queryable, searchParams.PageNumber!.Value, searchParams.PageSize!.Value);


                var paginationMetadata = pagedList.CreatePaginationMetadata(linkGenerator, httpContext, "GetSets");
                httpContext.Response.Headers.Append("Pagination", JsonSerializer.Serialize(paginationMetadata));

                return Results.Ok(pagedList.Select(set => set.ToDto()));

            }).WithName("GetSets");*/

            setsGroup.MapGet("sets/{setId}", async (int comedianId, int setId, ForumDbContext dbContext, CancellationToken cancellationToken) =>
            {

                var comedian = await dbContext.Comedians.FirstOrDefaultAsync(t => t.Id == comedianId, cancellationToken);

                if (comedian == null)
                {
                    return Results.NotFound($"No comedian found with ID {comedianId}.");
                }

                var set = await dbContext.Sets.FirstOrDefaultAsync(p => p.Id == setId && p.Comedian.Id == comedianId, cancellationToken);
                if (set == null)
                {
                    return Results.NotFound();
                }

                return Results.Ok(set.ToDto());
            });

            setsGroup.MapPost("/sets", async (int comedianId, CreateSetDto dto, ForumDbContext dbContext) => {
                var comedian = await dbContext.Comedians.FindAsync(comedianId);

                if (comedian == null)
                {
                    return Results.NotFound($"No comedian found with ID {comedianId}.");
                }

                var set = new Set { Title = dto.Title, Body = dto.Body, CreatedAt = DateTimeOffset.UtcNow, Comedian = comedian };
                dbContext.Sets.Add(set);

                await dbContext.SaveChangesAsync();

                return TypedResults.Created($"api/comedians/{comedianId}/sets/{set.Id}", set.ToDto());
            });

            setsGroup.MapPut("/sets/{setId}", async (int comedianId, int setId, UpdateSetDto dto, ForumDbContext dbContext) =>
            {
                var set = await dbContext.Sets.FirstOrDefaultAsync(s => s.Comedian.Id == comedianId && s.Id == setId);
                if (set == null)
                {
                    return Results.NotFound($"No set found with ID {setId}.");
                }


                set.Body = dto.Body;

                dbContext.Sets.Update(set);
                await dbContext.SaveChangesAsync();

                return Results.Ok(set.ToDto());
            });

            setsGroup.MapDelete("/sets/{setId}", async (int comedianId, int setId, ForumDbContext dbContext) =>
            {
                var set = await dbContext.Sets.FirstOrDefaultAsync(s => s.Comedian.Id == comedianId && s.Id == setId);
                if (set == null)
                {
                    return Results.NotFound($"No set found with ID {setId}.");
                }
                dbContext.Sets.Remove(set);
                await dbContext.SaveChangesAsync();

                return Results.NoContent();
            });
        }

        public static void AddCommentsApi(this WebApplication app)
        {
            var commentsGroup = app.MapGroup("/api/comedians/{comedianId}/sets/{setId}").AddFluentValidationAutoValidation();

            commentsGroup.MapGet("comments", async (int comedianId, int setId, ForumDbContext dbContext, CancellationToken cancellationToken) =>
            {

                //return (await dbContext.Sets.ToListAsync(cancellationToken)).Select(set => set.ToDto());
                var set = await dbContext.Sets
                      .FirstOrDefaultAsync(set => set.Comedian.Id == comedianId && set.Id == setId, cancellationToken);

                if (set == null)
                {
                    return Results.NotFound($"Set with ID {setId} not found for comedian with ID {comedianId}.");
                }

                // Now, fetch the comments for this set
                var comments = await dbContext.Comments
                    .Where(comment => comment.Set.Id == setId)
                    .ToListAsync(cancellationToken);

                if (!comments.Any())
                {
                    return Results.NotFound($"No comments found for set with ID {setId}.");
                }
                return Results.Ok(comments.Select(comment => comment.ToDto()));

            });

            commentsGroup.MapGet("/comments/{commentId}", async (int comedianId, int setId, int commentId, ForumDbContext dbContext, CancellationToken cancellationToken) =>
            {
                // Checking if the comedian exists
                var comedian = await dbContext.Comedians.FirstOrDefaultAsync(c => c.Id == comedianId, cancellationToken);

                if (comedian == null)
                {
                    return Results.NotFound($"Comedian with ID {comedianId} not found.");
                }

                // Checking if the set exists for the comedian
                var set = await dbContext.Sets.FirstOrDefaultAsync(s => s.Id == setId && s.Comedian.Id == comedianId, cancellationToken);
                if (set == null)
                {
                    return Results.NotFound($"Set with ID {setId} not found for comedian with ID {comedianId}.");
                }
                //Fetch the specific comment

                var comment = await dbContext.Comments.FirstOrDefaultAsync(c => c.Id == commentId && c.Set.Id == setId, cancellationToken);
                if (comment == null)
                {
                    return Results.NotFound($"Comment with ID {commentId} not found for set with ID {setId}.");
                }

                return Results.Ok(comment.ToDto());
            });

            commentsGroup.MapPost("/comments", async (int comedianId, int setId, CreateCommentDto dto, ForumDbContext dbContext) => {
                var comedian = await dbContext.Comedians.FindAsync(comedianId);

                if (comedian == null)
                {
                    return Results.NotFound($"Comedian with ID {comedianId} not found.");
                }

                var set = await dbContext.Sets.FirstOrDefaultAsync(s => s.Id == setId && s.Comedian.Id == comedianId);
                if (set == null)
                {
                    return Results.NotFound($"Set with ID {setId} not found for comedian with ID {comedianId}.");
                }

                var comment = new Comment { Content = dto.Content, CreatedAt = DateTimeOffset.UtcNow, Set = set };
                dbContext.Comments.Add(comment);

                await dbContext.SaveChangesAsync();

                return TypedResults.Created($"/api/comedians/{comedianId}/sets/{setId}/comments/{comment.Id}", comment.ToDto());
            });

            commentsGroup.MapPut("/comments/{commentId}", async (int comedianId, int setId, int commentId, UpdateCommentDto dto, ForumDbContext dbContext) =>
            {
                // Check if the comment exists and is associated with the given set and comedian
                var comment = await dbContext.Comments
                    .FirstOrDefaultAsync(c => c.Set.Id == setId && c.Set.Comedian.Id == comedianId && c.Id == commentId);

                if (comment == null)
                {
                    return Results.NotFound($"Comment with ID {commentId} not found");
                }

                comment.Content = dto.Content;

                dbContext.Comments.Update(comment);
                await dbContext.SaveChangesAsync();

                return Results.Ok(comment.ToDto());
            });

            commentsGroup.MapDelete("/comments/{commentId}", async (int comedianId, int setId, int commentId, ForumDbContext dbContext) =>
            {
                var comment = await dbContext.Comments.FirstOrDefaultAsync(c => c.Set.Id == setId && c.Set.Comedian.Id == comedianId && c.Id == commentId);
                if (comment == null)
                {
                    return Results.NotFound($"Comment with ID {commentId} not found");
                }
                dbContext.Comments.Remove(comment);
                await dbContext.SaveChangesAsync();

                return Results.NoContent();
            });
        }
    }
}
