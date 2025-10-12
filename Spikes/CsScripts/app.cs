#!/usr/bin/dotnet --

// app.cs
using System;

Console.WriteLine("Hello from .NET 10 preview!");

// Get current time and display it
var currentTime = DateTime.Now;
Console.WriteLine($"The current time is: {currentTime}");

// Simple calculation
int a = 10;
int b = 20;
Console.WriteLine($"{a} + {b} = {a + b}");
