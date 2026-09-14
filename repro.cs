#:sdk Microsoft.NET.Sdk.Web
#:package HotChocolate.AspNetCore@16.6.4
#:package HotChocolate.Types.Analyzers@16.6.4

// `dotnet run repro.cs` prints the schema. Same class, same resolver, one attribute apart:
//
//   type Widget    { id: String! }               <- `size` is gone, silently
//   type Doohickey { id: String! size: Int! }

using HotChocolate;
using HotChocolate.Execution;
using HotChocolate.Types;
using Microsoft.Extensions.DependencyInjection;
using Repro;

[assembly: Module("ReproTypes")]

var executor = await new ServiceCollection()
                     .AddGraphQLServer()
                     .AddReproTypes()
                     .BuildRequestExecutorAsync();

Console.WriteLine(executor.Schema.ToString());

namespace Repro
{
    public sealed class Widget
    {
        public string Id => "w";

        public int Size => 1;
    }

    public sealed class Doohickey
    {
        public string Id => "d";

        public int Size => 1;
    }

    [QueryType]
    public static partial class Query
    {
        public static Widget GetWidget() => new();

        public static Doohickey GetDoohickey() => new();
    }

    // BROKEN — [ObjectType<T>] is what the HC0096 hint asks you to migrate to.
    [ObjectType<Widget>]
    public static partial class WidgetNode
    {
        [BindMember(nameof(Widget.Size))]
        public static int GetSize([Parent] Widget parent) => parent.Size * 2;
    }

    // WORKS — the attribute HC0096 wants you to leave behind.
    [ExtendObjectType<Doohickey>]
    public static class DoohickeyExtension
    {
        [BindMember(nameof(Doohickey.Size))]
        public static int GetSize([Parent] Doohickey parent) => parent.Size * 2;
    }
}
