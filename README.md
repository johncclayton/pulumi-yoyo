# Pulumi YOYO

A wrapper around your existing Pulumi CLI (and Automation API) tools that helps you to up/destroy a hierarchy of
Pulumi Stacks together in a single command.

Batteries included:
- up (or destroy) the whole series of stacks with just one command
- re-uses existing authentication mechanisms
- per-project configuration using simple JSON
- ability to perform pre actions at each level of the stack hierarchy 

## Install

dotnet tool install --global Pulumi.Yoyo

## Usage

There is more information in the docs about how to set up a project file.  Lets just assume you have a project that brings 
up a cluster, a database and an application and that this is 3 stacks. 

E.g. something like this, where ``app`` requires ``mssql`` and ``mssql`` requires ``cluster``. 
```markdown
── Project Graph ──
└── app: test-std-example-app-dev
    └── mssql: test-std-example-mssql-dev
        └── cluster: test-std-cluster-dev
```

Yoyo requires you to name each of your stacks, this makes it much easier to run yoyo commands on any of the stacks
in the hierarchy.  

You can use Yoyo to bring the whole lot up in one go.

```bash
yoyo up

Would run command: pulumi up --skip-preview -s test-std-cluster-dev --non-interactive
Would run command: pulumi up --skip-preview -s test-std-example-mssql-dev --non-interactive
Would run command: pulumi up --skip-preview -s test-std-example-app-dev --non-interactive
```

You can also use Yoyo to bring up just the stack called `database`, notice the use of the short-name "mssql" and the --to-stack option.

```bash
yoyo up -to mssql
```

Or just the `cluster` stack...

```bash
yoyo up -to cluster
```

Or destroy the whole lot

```bash
yoyo destroy

Would run command: pulumi destroy --yes -s test-std-example-app-dev --non-interactive
Would run command: pulumi destroy --yes -s test-std-example-mssql-dev --non-interactive
Would run command: pulumi destroy --yes -s test-std-cluster-dev --non-interactive
```

# But my stack is a snowflake, I want to XYZ

You can!

Lets imagine that I do not want to destroy the cluster, instead I just want to shut it down.  This saves me money, and also time - I don't 
pay for a running cluster - just the allocated disks.

The way to do this is to write a pre-stage.[ps1|bash|python|node] script, and place this in a very particular location so 
that Yoyo can find it. 

## Location of pre-stage scripts

The location of the scripts follows this format: 
    
    ```
    <environment directory>.yoyo/<environment name>/<stack short name>/pre-stage.[ps1|bash|python|node]
    ```

The environment directory is _either_ declared in the EnvironmentConfig section of the JSON configuration, in which
case this value is used, or it is the current working directory.

Here's a full example of a configuration: 

```json
{
  "Name": "dev",
  "Environment": {
    "SubscriptionName": "something you like, not used during yoyo commands",
    "DefaultDirectoryForEnvironment": "g:/src/quickstart/testing"
  },
  "Stacks": [
    {
      "ShortName": "app",
      "DirectoryPath": "example-app",
      "FullStackName": "test-std-example-app-dev",
      "DependsOn": ["mssql", "cluster"]
    },
    {
      "ShortName": "mssql",
      "DirectoryPath": "example-mssql",
      "FullStackName": "test-std-example-mssql-dev",
      "DependsOn": ["cluster"]
    },
    {
      "ShortName": "cluster",
      "DirectoryPath": "example-cluster",
      "FullStackName": "test-std-cluster-dev"
    }
  ]
}
```

In this case the default directory for the environment is `g:/src/quickstart/testing` and the environment name is `dev`.

Therefore Yoyo will look in the following locations for the pre-stage scripts:

```
g:/src/quickstart/testing/.yoyo/dev/app/pre-stage.ps1
g:/src/quickstart/testing/.yoyo/dev/mssql/pre-stage.ps1
g:/src/quickstart/testing/.yoyo/dev/cluster/pre-stage.ps1
```

## Parameters to pre-stage scripts

No parameters are passed to the pre-stage scripts, but they can use environment variables to get information about the stack
that is being operated on.

The following environment variables are set _in addition to every current environment variable being injected into the script process_:

| Variable                    | Description                         | Example                                  |
|-----------------------------|-------------------------------------|------------------------------------------|
| YOYO_STAGE                  | The stage name                      | preview, up, destroy                     |
| YOYO_STACK_SHORT_NAME       | The short name of the stack         | app, mssql, cluster                      |
| YOYO_STACK_FULL_STACK_NAME  | The full name of the stack          | test-std-example-app-dev                 |
| YOYO_OPTION_DRYRUN          | True if the command is a dryrun     | True                                     |
| YOYO_OPTION_CONFIG          | The path to the config file         | g:/src/quickstart/testing/my-config.json |
| YOYO_OPTION_FROM_STACK      | The stack to start from             | app, mssql, cluster                      |
| YOYO_OPTION_TO_STACK        | The stack to finish on              | app, mssql, cluster                      |
| YOYO_OPTION_VERBOSE         | True if the command is verbose      | True                                     |
 | YOYO_WORKING_DIRECTORY     | The directory the command is run in | g:/src/quickstart/testing/example-app    |

