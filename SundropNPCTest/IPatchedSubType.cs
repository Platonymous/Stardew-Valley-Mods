using System;
namespace SundropNPCTest
{
    public interface IPatchedSubType
    {
        Type SubTypeOf { get; }

        bool ShouldPatch { get; }
    }
}
