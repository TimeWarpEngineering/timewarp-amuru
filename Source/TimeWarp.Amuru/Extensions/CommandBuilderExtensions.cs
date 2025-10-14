namespace TimeWarp.Amuru;

/// <summary>
/// Extension methods for ICommandBuilder to provide conditional and functional configuration patterns.
/// </summary>
public static class CommandBuilderExtensions
{
  /// <summary>
  /// Conditionally applies a configuration to the builder.
  /// Allows for fluent conditional configuration without breaking the method chain.
  /// </summary>
  /// <typeparam name="TBuilder">The builder type</typeparam>
  /// <param name="builder">The builder instance</param>
  /// <param name="condition">The condition to evaluate</param>
  /// <param name="configure">The configuration action to apply if the condition is true</param>
  /// <returns>The builder instance for method chaining</returns>
  /// <example>
  /// builder.When(version != null, b => b.WithVersion(version!))
  /// </example>
#pragma warning disable CA1716 // Identifiers should not match keywords - 'When' is the appropriate fluent method name
  public static TBuilder When<TBuilder>(this TBuilder builder, bool condition, Func<TBuilder, TBuilder> configure)
    where TBuilder : ICommandBuilder<TBuilder>
  {
    return condition ? configure(builder) : builder;
  }
#pragma warning restore CA1716

  /// <summary>
  /// Conditionally applies a configuration to the builder when the value is not null.
  /// The non-null value is passed to the configuration function.
  /// </summary>
  /// <typeparam name="TBuilder">The builder type</typeparam>
  /// <typeparam name="TValue">The value type</typeparam>
  /// <param name="builder">The builder instance</param>
  /// <param name="value">The value to check for null</param>
  /// <param name="configure">The configuration action to apply if the value is not null, receiving the value</param>
  /// <returns>The builder instance for method chaining</returns>
  /// <example>
  /// builder.WhenNotNull(version, (b, v) => b.WithVersion(v))
  /// </example>
  public static TBuilder WhenNotNull<TBuilder, TValue>(
    this TBuilder builder,
    TValue? value,
    Func<TBuilder, TValue, TBuilder> configure)
    where TBuilder : ICommandBuilder<TBuilder>
  {
    return value is not null ? configure(builder, value) : builder;
  }

  /// <summary>
  /// Conditionally applies a configuration to the builder when the condition is false.
  /// Inverse of When().
  /// </summary>
  /// <typeparam name="TBuilder">The builder type</typeparam>
  /// <param name="builder">The builder instance</param>
  /// <param name="condition">The condition to evaluate</param>
  /// <param name="configure">The configuration action to apply if the condition is false</param>
  /// <returns>The builder instance for method chaining</returns>
  /// <example>
  /// builder.Unless(isProduction, b => b.WithVerbose())
  /// </example>
  public static TBuilder Unless<TBuilder>(this TBuilder builder, bool condition, Func<TBuilder, TBuilder> configure)
    where TBuilder : ICommandBuilder<TBuilder>
  {
    return !condition ? configure(builder) : builder;
  }

  /// <summary>
  /// Applies a configuration function to the builder unconditionally.
  /// Useful for extracting reusable configuration logic.
  /// </summary>
  /// <typeparam name="TBuilder">The builder type</typeparam>
  /// <param name="builder">The builder instance</param>
  /// <param name="configure">The configuration action to apply</param>
  /// <returns>The builder instance for method chaining</returns>
  /// <example>
  /// builder.Apply(AddProductionSettings)
  /// </example>
  public static TBuilder Apply<TBuilder>(this TBuilder builder, Func<TBuilder, TBuilder> configure)
    where TBuilder : ICommandBuilder<TBuilder>
  {
    return configure(builder);
  }

  /// <summary>
  /// Applies a configuration function for each item in the collection.
  /// </summary>
  /// <typeparam name="TBuilder">The builder type</typeparam>
  /// <typeparam name="TItem">The item type</typeparam>
  /// <param name="builder">The builder instance</param>
  /// <param name="items">The collection of items to iterate</param>
  /// <param name="configure">The configuration action to apply for each item</param>
  /// <returns>The builder instance for method chaining</returns>
  /// <example>
  /// builder.ForEach(sources, (b, source) => b.WithSource(source))
  /// </example>
  public static TBuilder ForEach<TBuilder, TItem>(
    this TBuilder builder,
    IEnumerable<TItem> items,
    Func<TBuilder, TItem, TBuilder> configure)
    where TBuilder : ICommandBuilder<TBuilder>
  {
    TBuilder result = builder;
    foreach (TItem item in items)
    {
      result = configure(result, item);
    }

    return result;
  }

  /// <summary>
  /// Executes a side effect action without modifying the builder.
  /// Useful for logging, debugging, or other side effects during configuration.
  /// </summary>
  /// <typeparam name="TBuilder">The builder type</typeparam>
  /// <param name="builder">The builder instance</param>
  /// <param name="action">The side effect action to execute</param>
  /// <returns>The builder instance for method chaining</returns>
  /// <example>
  /// builder.Tap(b => Console.WriteLine($"Configuring: {b}"))
  /// </example>
  public static TBuilder Tap<TBuilder>(this TBuilder builder, Action<TBuilder> action)
    where TBuilder : ICommandBuilder<TBuilder>
  {
    action(builder);
    return builder;
  }
}
