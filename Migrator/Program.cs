using Infrastructure;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddDbContext<JournalDbContext>(options =>
    options.UseSqlite("Data Source=/Users/yuan/MoodJournal.db")); // same as MAUI

var provider = services.BuildServiceProvider();