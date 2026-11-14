using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace ran.EventManagement.Plugins
{
	public static class EventPluginHelper
	{
		public static IPluginExecutionContext GetContext(
			IServiceProvider serviceProvider)
		{
			return (IPluginExecutionContext)serviceProvider.GetService(
				typeof(IPluginExecutionContext));
		}

		public static IOrganizationService GetService(
			IServiceProvider serviceProvider,
			IPluginExecutionContext context)
		{
			var factory = (IOrganizationServiceFactory)
				serviceProvider.GetService(
					typeof(IOrganizationServiceFactory));

			return factory.CreateOrganizationService(context.UserId);
		}

		public static ITracingService GetTracing(
			IServiceProvider serviceProvider)
		{
			return (ITracingService)serviceProvider.GetService(
				typeof(ITracingService));
		}

		public static object GetValue(
			Entity target,
			Entity preImage,
			string attributeName)
		{
			if (target != null && target.Contains(attributeName))
				return target[attributeName];

			if (preImage != null && preImage.Contains(attributeName))
				return preImage[attributeName];

			return null;
		}

		public static EntityReference GetReference(
			Entity target,
			Entity preImage,
			string attributeName)
		{
			return GetValue(target, preImage, attributeName)
				as EntityReference;
		}

		public static DateTime? GetDate(
			Entity target,
			Entity preImage,
			string attributeName)
		{
			object value = GetValue(target, preImage, attributeName);

			if (value is DateTime date)
				return date;

			return null;
		}

		public static int GetOption(
			Entity target,
			Entity preImage,
			string attributeName,
			int defaultValue = -1)
		{
			object value = GetValue(target, preImage, attributeName);

			if (value is OptionSetValue option)
				return option.Value;

			return defaultValue;
		}

		public static bool HasRelatedRecord(
			IOrganizationService service,
			string tableName,
			string lookupColumn,
			Guid parentId)
		{
			var query = new QueryExpression(tableName)
			{
				ColumnSet = new ColumnSet(false),
				TopCount = 1
			};

			query.Criteria.AddCondition(
				lookupColumn,
				ConditionOperator.Equal,
				parentId);

			return service.RetrieveMultiple(query).Entities.Count > 0;
		}

		public static void Throw(string message)
		{
			throw new InvalidPluginExecutionException(message);
		}
	}
}
