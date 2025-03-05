using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Features.Conversations.Models;

namespace FPTPlaygroundServer.Features.Conversations.Mappers;

public static class ConversationMapper
{
    public static ConversationResponse? ToConversationResponse(this Conversation? c)
    {
        if (c != null)
        {
            return new ConversationResponse
            {
                Id = c.Id,
                ConversationIndex = c.ConversationIndex,
                Name = c.Name,
                Type = c.Type,
                GroupImageUrl = c.GroupImageUrl,
                Status = c.Status,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
            };
        }
        return null;
    }
}
