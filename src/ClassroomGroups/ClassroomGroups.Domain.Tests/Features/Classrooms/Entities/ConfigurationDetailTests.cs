using ClassroomGroups.Domain.Features.Classrooms.Entities;

namespace ClassroomGroups.Domain.Tests.Features.Classrooms.Entities;

public class ConfigurationDetailTests
{
  [Fact]
  public void CalculateGroupCount_WhenBothNumberOfGroupsAndStudentsPerGroupAreProvided_ReturnsErrorResult()
  {
    // Arrange
    int? numberOfGroups = 5;
    int? studentsPerGroup = 4;
    int numCandidateStudents = 20;
    var configurationDetail = new ConfigurationDetail(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      "",
      "",
      [],
      []
    );

    // Act
    var (groupCount, errorResult) = configurationDetail.CalculateGroupCount(
      numberOfGroups,
      studentsPerGroup,
      numCandidateStudents
    );

    // Assert
    Assert.Equal(-1, groupCount);
    Assert.Equal("Cannot specify multiple group counts", errorResult?.ErrorMessage);
  }

  [Fact]
  public void CalculateGroupCount_WhenNumberOfGroupsIsNegative_ReturnsErrorResult()
  {
    // Arrange
    int? numberOfGroups = -1;
    int? studentsPerGroup = null;
    int numCandidateStudents = 20;
    var configurationDetail = new ConfigurationDetail(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      "",
      "",
      [],
      []
    );

    // Act
    var (groupCount, errorResult) = configurationDetail.CalculateGroupCount(
      numberOfGroups,
      studentsPerGroup,
      numCandidateStudents
    );

    // Assert
    Assert.Equal(-1, groupCount);
    Assert.Equal("Group count must be a positive number", errorResult?.ErrorMessage);
  }

  [Fact]
  public void CalculateGroupCount_WhenStudentsPerGroupIsNegative_ReturnsErrorResult()
  {
    // Arrange
    int? numberOfGroups = null;
    int? studentsPerGroup = -1;
    int numCandidateStudents = 20;
    var configurationDetail = new ConfigurationDetail(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      "",
      "",
      [],
      []
    );

    // Act
    var (groupCount, errorResult) = configurationDetail.CalculateGroupCount(
      numberOfGroups,
      studentsPerGroup,
      numCandidateStudents
    );

    // Assert
    Assert.Equal(-1, groupCount);
    Assert.Equal("Group count must be a positive number", errorResult?.ErrorMessage);
  }

  [Fact]
  public void CalculateGroupCount_WhenNumberOfGroupsIsGreaterThanOrEqualToStudentCount_ReturnsErrorResult()
  {
    // Arrange
    int? numberOfGroups = 21;
    int? studentsPerGroup = null;
    int numCandidateStudents = 20;
    var configurationDetail = new ConfigurationDetail(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      "",
      "",
      [],
      []
    );

    // Act
    var (groupCount, errorResult) = configurationDetail.CalculateGroupCount(
      numberOfGroups,
      studentsPerGroup,
      numCandidateStudents
    );

    // Assert
    Assert.Equal(-1, groupCount);
    Assert.Equal(
      "Group count must be less than the available student count",
      errorResult?.ErrorMessage
    );
  }

  [Fact]
  public void CalculateGroupCount_WhenStudentsPerGroupIsGreaterThanOrEqualToStudentCount_ReturnsErrorResult()
  {
    // Arrange
    int? numberOfGroups = null;
    int? studentsPerGroup = 21;
    int numCandidateStudents = 20;
    var configurationDetail = new ConfigurationDetail(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      "",
      "",
      [],
      []
    );

    // Act
    var (groupCount, errorResult) = configurationDetail.CalculateGroupCount(
      numberOfGroups,
      studentsPerGroup,
      numCandidateStudents
    );

    // Assert
    Assert.Equal(-1, groupCount);
    Assert.Equal(
      "Group count must be less than the available student count",
      errorResult?.ErrorMessage
    );
  }

  [Fact]
  public void CalculateGroupCount_WhenBothNumberOfGroupsAndStudentsPerGroupAreNull_ReturnsErrorResult()
  {
    // Arrange
    int? numberOfGroups = null;
    int? studentsPerGroup = null;
    int numCandidateStudents = 20;
    var configurationDetail = new ConfigurationDetail(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      "",
      "",
      [],
      []
    );

    // Act
    var (groupCount, errorResult) = configurationDetail.CalculateGroupCount(
      numberOfGroups,
      studentsPerGroup,
      numCandidateStudents
    );

    // Assert
    Assert.Equal(-1, groupCount);
    Assert.Equal(
      "Group by either number of groups or students per group",
      errorResult?.ErrorMessage
    );
  }

  [Fact]
  public void CalculateGroupCount_WhenNumberOfGroupsIsZero_ReturnsZeroGroups()
  {
    // Arrange
    int? numberOfGroups = 0;
    int? studentsPerGroup = null;
    int numCandidateStudents = 20;
    var configurationDetail = new ConfigurationDetail(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      "",
      "",
      [],
      []
    );

    // Act
    var (groupCount, errorResult) = configurationDetail.CalculateGroupCount(
      numberOfGroups,
      studentsPerGroup,
      numCandidateStudents
    );

    // Assert
    Assert.Equal(0, groupCount);
    Assert.Null(errorResult);
  }

  [Fact]
  public void CalculateGroupCount_WhenStudentsPerGroupIsZero_ReturnsZeroGroups()
  {
    // Arrange
    int? numberOfGroups = null;
    int? studentsPerGroup = 0;
    int numCandidateStudents = 20;
    var configurationDetail = new ConfigurationDetail(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      "",
      "",
      [],
      []
    );

    // Act
    var (groupCount, errorResult) = configurationDetail.CalculateGroupCount(
      numberOfGroups,
      studentsPerGroup,
      numCandidateStudents
    );

    // Assert
    Assert.Equal(0, groupCount);
    Assert.Null(errorResult);
  }

  [Fact]
  public void CalculateGroupCount_WhenNumberOfGroupsIsProvided_ReturnsCorrectGroupCount()
  {
    // Arrange
    int? numberOfGroups = 3;
    int? studentsPerGroup = null;
    int numCandidateStudents = 20;
    var configurationDetail = new ConfigurationDetail(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      "",
      "",
      [],
      []
    );

    // Act
    var (groupCount, errorResult) = configurationDetail.CalculateGroupCount(
      numberOfGroups,
      studentsPerGroup,
      numCandidateStudents
    );

    // Assert
    Assert.Equal(3, groupCount);
    Assert.Null(errorResult);
  }

  [Fact]
  public void CalculateGroupCount_WhenStudentsPerGroupIsProvided_ReturnsCalculatedGroupCount()
  {
    // Arrange
    int? numberOfGroups = null;
    int? studentsPerGroup = 4;
    int numCandidateStudents = 20;
    var configurationDetail = new ConfigurationDetail(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      "",
      "",
      [],
      []
    );

    // Act
    var (groupCount, errorResult) = configurationDetail.CalculateGroupCount(
      numberOfGroups,
      studentsPerGroup,
      numCandidateStudents
    );

    // Assert
    Assert.Equal(5, groupCount); // 20 students / 4 students per group = 5 groups
    Assert.Null(errorResult);
  }
}
