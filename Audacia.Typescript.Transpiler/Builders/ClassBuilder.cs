using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Audacia.Typescript.Transpiler.Configuration;
using Audacia.Typescript.Transpiler.Documentation;
using Audacia.Typescript.Transpiler.Extensions;

namespace Audacia.Typescript.Transpiler.Builders
{
    public class ClassBuilder : TypeBuilder
    {
        private readonly IEnumerable<Type> _interfaces;
        private readonly IEnumerable<Type> _typeArguments;
        private readonly IEnumerable<PropertyInfo> _properties;

        public ClassBuilder(Type sourceType, InputSettings settings, XmlDocumentation documentation)
            : base(sourceType, settings, documentation)
        {
            _typeArguments = sourceType.GetGenericArguments();
            _interfaces = SourceType.GetDeclaredInterfaces()
                .Where(t => !t.Namespace.StartsWith(nameof(System)))
                .Where(i => Settings.Namespaces == null
                            || settings.Namespaces.Select(n => n.Name).Contains(i.Namespace));
            _properties = sourceType.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(mi => mi.MemberType == MemberTypes.Property)
                .Cast<PropertyInfo>();
        }

        public override Element Build()
        {
            var @class = new Class(SourceType.Name.Split('`').First()) {Modifiers = {Modifier.Export}};

            if (Inherits != null)
                @class.Extends = Inherits.TypescriptName();

            var classDocumentation = Documentation.ForClass(SourceType);
            
            if (classDocumentation != null)
                @class.Comment = classDocumentation.Summary;

            if (SourceType.IsAbstract) @class.Modifiers.Add(Modifier.Abstract);

            foreach (var @interface in _interfaces)
                @class.Implements.Add(@interface.TypescriptName());

            foreach (var typeArgument in _typeArguments)
                @class.TypeArguments.Add(typeArgument.TypescriptName());

            foreach (var property in _properties)
            {
                var element = new Property(property.Name.CamelCase(), property.PropertyType.TypescriptName());
                var propertyDocumentation = Documentation.ForMember(property);
                
                if (propertyDocumentation != null)
                    element.Comment = propertyDocumentation.Summary;
                
                @class.Members.Add(element);
            }

            ReportProgress(ConsoleColor.Green, "class", @class.Name);
            return @class;
        }
    }
}