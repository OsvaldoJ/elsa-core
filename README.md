![Web-based workflow designer](/doc/elsa-cover.png)

## Elsa Workflows

[![MyGet (with prereleases)](https://img.shields.io/myget/elsa/vpre/Elsa.Core.svg?label=myget)](https://www.myget.org/gallery/elsa)

Elsa Core is a workflows library that enables workflow execution in any .NET Core application.
Workflows can be defined not only using code but also as JSON, YAML or XML.

In addition, workflows can be visually designed using [Elsa Designer](https://github.com/elsa-workflows/elsa-designer-html), a reusable & extensible HTML5 web component built with [StencilJS[(https://stenciljs.com/).

![Web-based workflow designer](/doc/workflow-sample-3.png)

## Programmatic Workflows

Workflows can be created programmatically and then executed using `IWorkflowInvoker`.

### Hello World
The following code snippet demonstrates creating a workflow with two custom activities from code and then invoking it:

```c#

// Define a strongly-typed workflow.
public class HelloWorldWorkflow : IWorkflow
{
    public void Build(IWorkflowBuilder builder)
    {
        builder
            .StartWith<HelloWorld>()
            .Then<GoodByeWorld>();
    }
}

// Setup a service collection.
var services = new ServiceCollection()
    .AddWorkflows()
    .AddActivity<HelloWorld>()
    .AddActivity<GoodByeWorld>()
    .BuildServiceProvider();

// Invoke the workflow.
var invoker = services.GetService<IWorkflowInvoker>();
await invoker.InvokeAsync<HelloWorldWorkflow>();

// Output:
// /> Hello World!
// /> Goodbye cruel World...
```

### Persistence

Workflows can be persisted using virtually any storage mechanism.
The following providers will be supported:

- In Memory
- File System
- SQL Server
- MongoDB
- CosmosDB

### Formats

Currently, workflows can be stored in YAML or JSON format.
The following demonstrates a simple workflow expressed in YAML and JSON, respectively:

**YAML**
```yaml
activities:
- name: WriteLine
  id: activity-1
  textExpression:  
    syntax: PlainText
    expression: Hi! What's your name?
- name: ReadLine
  id: activity-2
  argumentName: name
- name: WriteLine
  id: activity-3
  textExpression:
    syntax: JavaScript
    expression: '`Nice to meet you, ${name}!`'
connections:
- source:
    activityId: activity-1
    name: Done
  target:
    activityId: activity-2
- source:
    activityId: activity-2
    name: Done
  target:
    activityId: activity-3
```

**JSON**
```json
{
  "activities": [
    {
      "name": "WriteLine",
      "id": "activity-1",
      "textExpression": {
        "syntax": "PlainText",
        "expression": "Hi! What's your name?"
      }
    },
    {
      "id": "activity-2",
      "name": "ReadLine",
      "argumentName": "name"
    },
    {
      "name": "WriteLine",
      "id": "activity-3",
      "textExpression": {
        "syntax": "JavaScript",
        "expression": "`Nice to meet you, ${name}!`"
      }
    }
  ],
  "connections": [
    {
      "source": {
        "activityId": "activity-1",
        "name": "Done"
      },
      "target": {
        "activityId": "activity-2"
      }
    },
    {
      "source": {
        "activityId": "activity-2",
        "name": "Done"
      },
      "target": {
        "activityId": "activity-3"
      }
    }
  ]
}
```

The following demonstrates loading a workflow from a YAML string:

```c#
// Setup a service collection and use the FileSystemProvider for both workflow definitions and workflow instances.
var services = new ServiceCollection()
    .AddWorkflowsInvoker()
    .AddConsoleActivities()
    .AddSingleton(Console.In)
    .BuildServiceProvider();

// Load the data and specify data format.
var data = Resources.SampleWorkflowDefinition;
var format = YamlTokenFormatter.FormatName; // "YAML"

// Deserialize the workflow from data.
var serializer = services.GetService<IWorkflowSerializer>();
var workflowDefinition = await serializer.DeserializeAsync(data, format, CancellationToken.None);

// Invoke the workflow.
var invoker = services.GetService<IWorkflowInvoker>();
await invoker.InvokeAsync(workflowDefinition);
```

## Long Running Workflows

Elsa has native support for long-running workflows. As soon as a workflow is halted because of some blocking activity, the workflow is persisted.
When the appropriate event occurs, the workflow is loaded from the store and resumed. 

## Why Elsa Workflows?

One of the main goals of Elsa is to **enable workflows in any .NET application** with **minimum effort** and **maximum extensibility**.
This means that it should be easy to integrate workflow capabilities into your own application.

### What about Azure Logic Apps?

As powerful and as complete Azure Logic Apps is, it's available only as a managed service in Azure. Elsa on the other hand allows you to host it not only on Azure, but on any cloud provider that supports .NET Core. And of course you can host it on-premise.

Although you can implement long-running workflows with Logic Apps, you would typically do so with splitting your workflow with multiple Logic Apps where one workflow invokes the other. This can make the logic flow a bit hard to follow.
with Elsa, you simply add triggers anywhere in the workflow, making it easier to have a complete view of your application logic. And if you want, you can still invoke other workflows form one workflow.

### What about Windows Workflow Foundation?

I've always liked Windows Workflow Foundation, but unfortunately [development appears to have halted](https://forums.dotnetfoundation.org/t/what-is-the-roadmap-of-workflow-foundation/3066).
Although there's an effort being made to [port WF to .NET Standard](https://github.com/dmetzgar/corewf), there are a few reasons I prefer Elsa:

- Elsa intrinsically supports triggering events that starts new workflows and resumes halted workflow instances in an easy to use manner. E.g. `workflowHost.TriggerWorkflowAsync("HttpRequestTrigger");"` will start and resume all workflows that either start with or are halted on the `HttpRequestTrigger`. 
- Elsa has a web-based workflow designer. I once worked on a project for a customer that was building a huge SaaS platform. One of the requirements was to provide a workflow engine and a web-based editor. Although there are commercial workflow libraries and editors out there, the business model required open-source software. We used WF and the re-hosted Workflow Designer. It worked, but it wasn't great.

### What about Orchard Workflows?

Both [Orchard](http://docs.orchardproject.net/en/latest/Documentation/Workflows/) and [Orchard Core](https://orchardcore.readthedocs.io/en/dev/OrchardCore.Modules/OrchardCore.Workflows/) ship with a powerful workflows module, and both are awesome.
In fact, Elsa Workflows is taken & adapted from Orchard Core's Workflows module. Elsa uses a similar model, but there are some differences:  

- Elsa Workflows is completely decoupled from web, whereas Orchard Core Workflows is coupled to not only the web, but also the Orchard Core Framework itself.
- Elsa Workflows can execute in any .NET Core application without taking a dependency on any Orchard Core packages.

## Features

The following lists some of Elsa's key features:

- **Small, simple and fast**. The library should be lean & mean, meaning that it should be **easy to use**, **fast to execute** and **easy to extend** with custom activities.
- It must be a set of **libraries**. This allows me to create my application anyway I like, and implement workflow capabilities as I see fit. Thanks to ASP.NET Core's application model however, creating a workflow designer & workflow host is as simple as referencing the right packages and making a few calls. 
- Invoke arbitrary workflows as if they were **functions of my application**.
- Trigger events that cause the appropriate workflows to **automatically start/resume** based on that event.
- Support **long-running workflows**. When a workflow executes and encounters an activity that requires e.g. user input, the workflow will halt, be persisted and go out of memory until it's time to resume. this could be a few seconds later, a few minutes, hours, days or even years.
- **Correlate** workflows with application-specific data. This is a key requirement for long-running workflows.
- Store workflows in a **file-based** format so I can make it part of source-control.
- Store workflows in a **database** when I don't want to make them part of source control.
- A **web-based designer**. Whether I store my workflows on a file system or in a database, and whether I host the designer online or only on my local machine, I need to be able to edit my workflows.
- Configure workflow activities with **expressions**. Oftentimes, information being processed by a workflow is dynamic in nature, and activities need a way to interact with this information. Workflow expressions allow for this.
- **Extensible** with application-specific **activities**, **custom stores** and **scripting engines**.
- Invoke other workflows. This allows for invoking reusable application logic from various workflows. Like invoking general-purpose functions from C# without having to duplicate code.
- **View & analyze** executed workflow instances. I want to see **which path** a workflow took, its **runtime state**, where it **faulted** and **compensate** faulted workflows.
- **Embed** the web-based workflow designer in **my own dashboard** application. This gives me the option of creating a single Workflow Host that runs all of my application logic, but also the option of hosting a workflows runtime in individual micro services (allowing for orchestration as well as choreography).
- **Separation of concerns**: The workflow core library, runtime and designer should all be separated. I.e. when the workflow host should not have a dependency on the web-based designer. This allows one for example to implement a desktop-based designer, or not use a designer at all and just go with YAML files. The host in the end only needs the workflow definitions and access to persistence stores.
- **On premise** or **managed** in the cloud - both scenarios are supported, because Elsa is just a set of NuGet packages that you reference from your application.

## How to use Elsa

Elsa is distributed as a set of NuGet packages, which makes it easy to add to your application.
When working with Elsa, you'll typically want to have at least two applications:

1. An ASP.NET Core application to host the workflows designer.
2. A .NET application that executed workflows

### Setting up a Workflow Designer ASP.NET Core Application

TODO: describe all the steps to add packages and register services.

### Setting up a Workflow Host .NET Application 

TODO: describe all the steps to add packages and register services.

## Running Elsa Workflows Dashboard

In order to run Elsa on your local machine, follow these steps:

1. Clone the repository.
2. Run NPM install on all folders containing packages.json (or run `node npm-install.js` - a script in the root that recursively installs the Node packages)
3. Open a shell and navigate to `src/samples/SampleDashboard.Web` and run `dotnet run`.
4. Navigate to https://localhost:44397/

## Running Elsa Workflows Host

(TODO)

## Roadmap

(TODO)
 
- Describe all the features (core engine, runtime, webbased designer, YAML, scripting, separation of designer from invoker from engine).
- Describe various use cases.
- Describe how to use.
- Describe architecture.
- Describe how to implement (custom host, custom dashboard).
- Implement more activities
- Implement integration with Orchard Core (separate repo)
- Detailed documentation
- Open API Activity Harvester
- MassTransit Activity Harvester
- RabbitMQ Activities
- Azure Service Bus Activities
- Automatic UI for Activity Editor

