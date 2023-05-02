/*
appsettings.json

<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net7.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
 */
using System.Reflection;

//----------------------------- 抽象度レベル 高 (ライブラリー) -----------------------------

static class EnumerableExtensions
{
    public static void ForEach<TElement>(this IEnumerable<TElement> @this, Action<TElement> action)
    {
        foreach (var element in @this)
            action(element);
    }
}

static class DIContainer
{
    static Dictionary<string, Type> map = new ();

    public static void Register(Assembly assembly, Func<Type, bool> isValid)
        => GetValidTypes(assembly, isValid).ForEach(type => Register(type));

    static IEnumerable<Type> GetValidTypes(Assembly assembly, Func<Type, bool> isValid)
        => assembly.GetTypes()
                   .Where(type => type.IsPublic && !type.IsAbstract && isValid(type));

    public static T? Resolve<T>(string name, params object?[]? arguments)
        => map.TryGetValue(name, out var type)
           ? (T?)Activator.CreateInstance(type, arguments)
           : default(T);

    static void Register(Type type) => map[type.Name] = type;
}

//----------------------------- 抽象度レベル 中 (フレームワーク) -----------------------------

class Menu
{
    readonly string[] commandTexts;

    public Menu(IEnumerable<string> commandTexts) => this.commandTexts = commandTexts.ToArray();

    public string GetOption()
    {
        for (; ;) {
            Console.WriteLine(this);
            var character = Console.ReadKey().KeyChar;
            Console.WriteLine();
            var commandText = ToCommandText(character);
            if (commandText is not null)
                return commandText;
        }

        string? ToCommandText(char character)
        {
            var index = character - '1';
            return 0 <= index && index < commandTexts.Length ? commandTexts[index] : null;
        }
    }

    public override string ToString()
    {
        var index = 0;
        var numberedCommandTexts = commandTexts.Select(commandText => $"{++index}: {commandText}");
        return $"{string.Join(", ", numberedCommandTexts)}:";
    }
}

public class ModelBase
{}

public class Application
{
    const string commandPostfix = "Command";
    const string modelPostfix   = "Model";

    public static void Run(IEnumerable<string> commandTexts)
    {
        var menu = new Menu(commandTexts);
        RegisterCommands();
        RegisterModels();
        MainLoop(menu);
    }

    static void RegisterCommands()
        => DIContainer.Register(typeof(Application).Assembly, type => type.Name.EndsWith(commandPostfix));

    static void RegisterModels()
        => DIContainer.Register(typeof(Application).Assembly, type => type.Name.EndsWith(modelPostfix) && type.IsAssignableTo(typeof(ModelBase)));

    static void MainLoop(Menu menu)
    {
        for (; ;) {
            var commandText = menu.GetOption();
            var command     = DIContainer.Resolve<object>($"{commandText}{commandPostfix}");
            if (command is not null)
                Execute(command, DIContainer.Resolve<ModelBase>($"{commandText}{modelPostfix}"));
        }

        static void Execute(object command, ModelBase? model)
            => ((dynamic)command).Execute(model);
    }
}

//----------------------------- 抽象度レベル 低 (アプリケーション) -----------------------------

public class AddStaffCommand
{
    public void Execute(ModelBase? model)
        => Console.WriteLine($"AddStaffCommand.Execute({model})");
}

public class UpdateStaffCommand
{
    public void Execute(ModelBase? model)
        => Console.WriteLine($"UpdateStaffCommand.Execute({model})");
}

public class AddStaffModel : ModelBase
{
    public override string ToString() => "AddStaffModel";
}

public class UpdateStaffModel : ModelBase
{
    public override string ToString() => "UpdateStaffModel";
}

class Program
{
    static void Main() => Application.Run(new[] { "AddStaff", "UpdateStaff" });
}
