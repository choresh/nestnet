# NestNet.Cli

A command-line tool for generating well-structured ASP.NET Core microservices with standardized architecture and best practices.

## For CLI Maintainers

### Development Prerequisites
- Windows 10 or later
- .NET 7.0 SDK or later
- .NET IDE (Visual Studio, Rider, etc.)

### Building/Deployment
If you use Visual Studio:
1. Build solution
2. Generate CLI executable:
   ```
   Right-click NestNet.Cli project → Publish → Publish
   ```
3. Create installer:
   ```
   Right-click NestNet.Cli.Installer → Build
   ```

### Known Issues
- 'Just Me' installation option doesn't work (use 'Everyone')

### Backlog Issues
- Support 'Just Me' installation option
- Support for migrations (trigered by entity stucture change?)
- Support views generation?
- Support other API protocols, e.g, pub/sub (simmilar to controler)
- Support SDK generation
- Support more filter operators
- Support/validate more ORM-related capabilities (relations, indexes)
- Support other ORMs (not EF, e.g. Dapper, RepoDb)?
- Support other database providers (not MS SQL Server)

### Documentation
- See [Project Guide](./Data/Templates/Doc/README.md) for generated project details

### Getting Help
- Check the [GitHub Issues (TODO)](https://github.com/yourusername/nestnet/issues)
- Join our [Discord Community (TODO)](https://discord.gg/yourdiscord)
- Review the [FAQ (TODO)](https://your-docs-url.com/faq)