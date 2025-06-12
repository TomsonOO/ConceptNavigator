using QuizGenerationService.Domain.ValueObjects;

namespace QuizGenerationService.Domain.Models;

public class ConceptMap
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Topic { get; set; } = string.Empty;
    public string? Book { get; set; }
    public SessionId? SessionId { get; set; }
    public Language Language { get; set; } = Language.Polish;
    public List<ConceptNode> Nodes { get; set; } = new();
    public List<ConceptRelationship> Relationships { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? GenerationSource { get; set; }

    public static ConceptMap Create(string topic, Language language, string? book = null, SessionId? sessionId = null)
    {
        if (string.IsNullOrWhiteSpace(topic))
            throw new ArgumentException("Topic cannot be empty", nameof(topic));

        return new ConceptMap
        {
            Topic = topic,
            Book = book,
            SessionId = sessionId,
            Language = language,
            GenerationSource = sessionId != null ? "session" : "direct"
        };
    }

    public void AddNode(ConceptNode node)
    {
        if (Nodes.Any(n => n.Name.Equals(node.Name, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }
        
        Nodes.Add(node);
    }

    public void AddRelationship(ConceptRelationship relationship)
    {
        if (!NodeExists(relationship.SourceConceptId) || !NodeExists(relationship.TargetConceptId))
        {
            throw new ArgumentException("Both source and target concepts must exist in the map");
        }

        var existingRelationship = Relationships.FirstOrDefault(r =>
            (r.SourceConceptId == relationship.SourceConceptId && r.TargetConceptId == relationship.TargetConceptId) ||
            (relationship.IsBidirectional && r.SourceConceptId == relationship.TargetConceptId && r.TargetConceptId == relationship.SourceConceptId));

        if (existingRelationship == null)
        {
            Relationships.Add(relationship);
        }
    }

    public ConceptNode? GetNode(string nodeId)
    {
        return Nodes.FirstOrDefault(n => n.Id == nodeId);
    }

    public ConceptNode? GetNodeByName(string name)
    {
        return Nodes.FirstOrDefault(n => n.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public List<ConceptNode> GetCoreNodes()
    {
        return Nodes.Where(n => n.IsCorePhilosophicalConcept && n.IsHighImportance)
                   .OrderByDescending(n => n.ImportanceScore)
                   .ToList();
    }

    public List<ConceptNode> GetNodesByType(ConceptType type)
    {
        return Nodes.Where(n => n.Type == type)
                   .OrderByDescending(n => n.ImportanceScore)
                   .ToList();
    }

    public List<ConceptRelationship> GetRelationshipsForNode(string nodeId)
    {
        return Relationships.Where(r => r.SourceConceptId == nodeId || r.TargetConceptId == nodeId).ToList();
    }

    public List<ConceptRelationship> GetStrongRelationships()
    {
        return Relationships.Where(r => r.IsStrongRelationship)
                           .OrderByDescending(r => r.Strength)
                           .ToList();
    }

    public Dictionary<ConceptType, List<ConceptNode>> GetNodesByTypeGrouped()
    {
        return Nodes.GroupBy(n => n.Type)
                   .ToDictionary(g => g.Key, g => g.OrderByDescending(n => n.ImportanceScore).ToList());
    }

    public int GetNodeConnectivity(string nodeId)
    {
        return Relationships.Count(r => r.SourceConceptId == nodeId || r.TargetConceptId == nodeId);
    }

    public List<ConceptNode> GetMostConnectedNodes(int maxCount = 5)
    {
        return Nodes.OrderByDescending(n => GetNodeConnectivity(n.Id))
                   .ThenByDescending(n => n.ImportanceScore)
                   .Take(maxCount)
                   .ToList();
    }

    public bool NodeExists(string nodeId)
    {
        return Nodes.Any(n => n.Id == nodeId);
    }

    public bool IsEmpty => !Nodes.Any();
    public int TotalNodes => Nodes.Count;
    public int TotalRelationships => Relationships.Count;
    public int CoreConceptCount => Nodes.Count(n => n.IsCorePhilosophicalConcept);
} 