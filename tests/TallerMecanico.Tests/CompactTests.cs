using Taller_Mecanico_Arqui.Application.Common;
using Taller_Mecanico_Arqui.Domain.Common;
using Taller_Mecanico_Arqui.Domain.Enums;
using Xunit;

namespace TallerMecanico.Tests;

/// <summary>
/// Minimal test suite using equivalence partitioning and boundary value analysis.
/// Covers each partition at least once - no redundant tests.
/// </summary>
public class CompactTests
{
    #region Result - Decision Coverage

    [Fact] public void Result_Success_ReturnsTrue() => Assert.True(Result.Success().IsSuccess);
    [Fact] public void Result_Failure_ReturnsFalse() => Assert.False(Result.Failure("E", "M").IsSuccess);
    [Fact] public void Result_IsFailure_IsOpposite() => Assert.True(Result.Failure("E", "M").IsFailure);

    [Fact] public void ResultT_Success_WithValue() 
    {
        var r = Result<string>.Success("ok");
        Assert.True(r.IsSuccess);
        Assert.Equal("ok", r.Value);
    }

    [Fact] public void ResultT_Failure_WithError() 
    {
        var r = Result<string>.Failure("ERR", "msg");
        Assert.True(r.IsFailure);
        Assert.Equal("ERR", r.ErrorCode);
    }

    [Fact] public void ResultT_Map_TransformsOnSuccess() 
        => Assert.Equal(20, Result<int>.Success(10).Map(x => x * 2).Value);

    [Fact] public void ResultT_Map_ReturnsFailureOnFailure() 
        => Assert.True(Result<int>.Failure("E", "M").Map(x => x * 2).IsFailure);

    [Fact] public void ResultT_Bind_ChainsResults() 
    {
        var r = Result<int>.Success(5).Bind(x => Result<string>.Success($"-{x}-"));
        Assert.Equal("-5-", r.Value);
    }

    [Fact] public void ResultT_Match_ReturnsOnSuccess() 
        => Assert.Equal("val", Result<string>.Success("val").Match(v => v, (c, m) => "err"));

    [Fact] public void ResultT_Match_ReturnsOnFailure() 
        => Assert.Equal("e:m", Result<string>.Failure("e", "m").Match(v => v, (c, m) => $"{c}:{m}"));

    [Fact] public void ResultT_GetValueOrThrow_ThrowsOnFailure() 
        => Assert.Throws<InvalidOperationException>(() => Result<string>.Failure("E", "M").GetValueOrThrow());

    [Fact] public void ResultT_GetValueOrDefault_ReturnsDefaultOnFailure() 
        => Assert.Equal("def", Result<string>.Failure("E", "M").GetValueOrDefault("def"));

    #endregion

    #region ValidationHelper - Equivalence Partitioning & Boundaries

    // Require - Boolean partition: true, false
    [Fact] public void Require_True_ReturnsSuccess() => Assert.True(ValidationHelper.Require(true, "E", "M").IsSuccess);
    [Fact] public void Require_False_ReturnsFailure() => Assert.True(ValidationHelper.Require(false, "E", "M").IsFailure);

    // RequireNotNull - Partitions: null, not null
    [Fact] public void RequireNotNull_Ref_Null_ReturnsFailure() => Assert.True(ValidationHelper.RequireNotNull<string>(null, "E", "M").IsFailure);
    [Fact] public void RequireNotNull_Ref_NotNull_ReturnsSuccess() => Assert.True(ValidationHelper.RequireNotNull("x", "E", "M").IsSuccess);
    

    // RequireNotEmpty - Partitions: null, empty, whitespace, valid
    [Fact] public void RequireNotEmpty_Null_ReturnsFailure() => Assert.True(ValidationHelper.RequireNotEmpty(null, "E", "M").IsFailure);
    [Fact] public void RequireNotEmpty_Empty_ReturnsFailure() => Assert.True(ValidationHelper.RequireNotEmpty("", "E", "M").IsFailure);
    [Fact] public void RequireNotEmpty_Whitespace_ReturnsFailure() => Assert.True(ValidationHelper.RequireNotEmpty("   ", "E", "M").IsFailure);
    [Fact] public void RequireNotEmpty_Valid_ReturnsSuccess() => Assert.True(ValidationHelper.RequireNotEmpty("x", "E", "M").IsSuccess);

    // ValidateEmail - Partitions: null, invalid, valid
    [Fact] public void ValidateEmail_Null_ReturnsFailure() => Assert.True(ValidationHelper.ValidateEmail(null).IsFailure);
    [Fact] public void ValidateEmail_Invalid_ReturnsFailure() => Assert.True(ValidationHelper.ValidateEmail("not-email").IsFailure);
    [Fact] public void ValidateEmail_Valid_ReturnsSuccess() => Assert.True(ValidationHelper.ValidateEmail("a@b.com").IsSuccess);

    // ValidatePhone - Boundaries: 8 digits (too short), 9 digits (valid), 10+ (too long)
    [Fact] public void ValidatePhone_8Digits_ReturnsFailure() => Assert.True(ValidationHelper.ValidatePhone(91234567).IsFailure);
    [Fact] public void ValidatePhone_9Digits_ReturnsSuccess() => Assert.True(ValidationHelper.ValidatePhone(912345678).IsSuccess);

    // ValidatePlate - Partitions: null, invalid, valid
    [Fact] public void ValidatePlate_Null_ReturnsFailure() => Assert.True(ValidationHelper.ValidatePlate(null).IsFailure);
    [Fact] public void ValidatePlate_Invalid_ReturnsFailure() => Assert.True(ValidationHelper.ValidatePlate("INVALID").IsFailure);
    [Fact] public void ValidatePlate_Valid_ReturnsSuccess() => Assert.True(ValidationHelper.ValidatePlate("ABC123").IsSuccess);

    // ValidateCiNumber - Boundaries: 99999 (min-1), 100000 (min), 99999999 (max), 100000000 (max+1)
    [Fact] public void ValidateCiNumber_99999_ReturnsFailure() => Assert.True(ValidationHelper.ValidateCiNumber(99999).IsFailure);
    [Fact] public void ValidateCiNumber_100000_ReturnsSuccess() => Assert.True(ValidationHelper.ValidateCiNumber(100000).IsSuccess);
    [Fact] public void ValidateCiNumber_99999999_ReturnsSuccess() => Assert.True(ValidationHelper.ValidateCiNumber(99999999).IsSuccess);
    [Fact] public void ValidateCiNumber_100000000_ReturnsFailure() => Assert.True(ValidationHelper.ValidateCiNumber(100000000).IsFailure);

    // ValidateCiComplement - Partitions: null, invalid, valid
    [Fact] public void ValidateCiComplement_Null_ReturnsSuccess() => Assert.True(ValidationHelper.ValidateCiComplement(null).IsSuccess);
    [Fact] public void ValidateCiComplement_Invalid_ReturnsFailure() => Assert.True(ValidationHelper.ValidateCiComplement("XX").IsFailure);
    [Fact] public void ValidateCiComplement_Valid_ReturnsSuccess() => Assert.True(ValidationHelper.ValidateCiComplement("1G").IsSuccess);

    // ParseEnum - Partitions: valid, invalid
    [Fact] public void ParseEnum_Valid_ReturnsSuccess() => Assert.Equal(TipoCliente.Regular, ValidationHelper.ParseEnum<TipoCliente>("Regular", "err").Value);
    [Fact] public void ParseEnum_Invalid_ReturnsFailure() => Assert.True(ValidationHelper.ParseEnum<TipoCliente>("Invalid", "err").IsFailure);

    // ParseNivelAcceso - Special case: "Total" maps to Completo
    [Fact] public void ParseNivelAcceso_Total_MapsToCompleto() => Assert.Equal(NivelAcceso.Completo, ValidationHelper.ParseNivelAcceso("Total").Value);

    // ValidateDateNotFuture - Boundaries: past, now, future
    [Fact] public void ValidateDateNotFuture_Past_ReturnsSuccess() => Assert.True(ValidationHelper.ValidateDateNotFuture(DateTime.UtcNow.AddDays(-1), "err").IsSuccess);
    [Fact] public void ValidateDateNotFuture_Future_ReturnsFailure() => Assert.True(ValidationHelper.ValidateDateNotFuture(DateTime.UtcNow.AddDays(1), "err").IsFailure);

    // ValidateYear - Boundaries: min-1, min, max, max+1
    [Fact] public void ValidateYear_BeforeRange_ReturnsFailure() => Assert.True(ValidationHelper.ValidateYear(1800, 1900, 2100, "err").IsFailure);
    [Fact] public void ValidateYear_InRange_ReturnsSuccess() => Assert.True(ValidationHelper.ValidateYear(2000, 1900, 2100, "err").IsSuccess);
    [Fact] public void ValidateYear_AfterRange_ReturnsFailure() => Assert.True(ValidationHelper.ValidateYear(2200, 1900, 2100, "err").IsFailure);

    // ValidateUnique - Partitions: duplicate, not duplicate
    [Fact] public void ValidateUnique_Duplicate_ReturnsFailure() => Assert.True(ValidationHelper.ValidateUnique(true, "err").IsFailure);
    [Fact] public void ValidateUnique_NotDuplicate_ReturnsSuccess() => Assert.True(ValidationHelper.ValidateUnique(false, "err").IsSuccess);

    // ValidateAccessLevelConfigured - Partitions: has value, null
    [Fact] public void ValidateAccessLevelConfigured_HasValue_ReturnsSuccess() => Assert.True(ValidationHelper.ValidateAccessLevelConfigured(NivelAcceso.Completo).IsSuccess);
    [Fact] public void ValidateAccessLevelConfigured_Null_ReturnsFailure() => Assert.True(ValidationHelper.ValidateAccessLevelConfigured(null).IsFailure);

    // ValidateAccessLevel - Partitions: sufficient, insufficient, null
    [Fact] public void ValidateAccessLevel_Sufficient_ReturnsSuccess() => Assert.True(ValidationHelper.ValidateAccessLevel(NivelAcceso.Completo, NivelAcceso.Parcial, "err").IsSuccess);
    [Fact] public void ValidateAccessLevel_Insufficient_ReturnsFailure() => Assert.True(ValidationHelper.ValidateAccessLevel(NivelAcceso.Parcial, NivelAcceso.Completo, "err").IsFailure);
    [Fact] public void ValidateAccessLevel_Null_ReturnsFailure() => Assert.True(ValidationHelper.ValidateAccessLevel(null, NivelAcceso.Parcial, "err").IsFailure);

    // RequirePositive - Boundaries: 0, positive, negative
    [Fact] public void RequirePositive_Zero_ReturnsFailure() => Assert.True(ValidationHelper.RequirePositive(0, "E", "M").IsFailure);
    [Fact] public void RequirePositive_Positive_ReturnsSuccess() => Assert.True(ValidationHelper.RequirePositive(1, "E", "M").IsSuccess);

    #endregion
}