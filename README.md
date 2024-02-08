# Pulumi YOYO

A wrapper around your existing Pulumi CLI tools that helps you to up/destroy a hierarchy of
Pulumi Stacks together in a single command.

Batteries included:
- up (or destroy) the whole series of stacks with just one command
- re-uses existing authentication mechanisms
- per-project configuration using simple JSON
- ability to perform pre actions at each level of the stack hierarchy 

## Why?

I found myself writing scripts to bring up a hierarchy of stacks.  I wanted to be able to bring up the whole
lot in one go, and also to be able to bring up just a part of the hierarchy. 

## What I'd like 

I would very much appreciate your feedback, even if its something like "finish the install commands please as I want to try this!".

## Install

You can install the tool from nuget (as a global exe/command) using the following :

```bash
$ dotnet tool install --global Pulumi.Yoyo
```

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

You can also use Yoyo to bring up just the stack called `mssql`, notice the use of the short-name "mssql" and the --to-stack option.

```bash
yoyo up -to-stack mssql
```

Or just the `cluster` stack...

```bash
yoyo up -to-stack cluster
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
pay for a running cluster - just the allocated disks.  So I need a way to: 

1. Stop the cluster
2. Avoid running the pulumi destroy command 

The way to do this is to write a pre-stage script, and place this in a very particular location so 
that Yoyo can find it. 

Yoyo will then run your pre-stage script, and using the exit code determine whether or not the stage's pulumi 
command should be run.

## How it works

Yoyo will look for scripts (see below for the names/locations).  If the scripts are found, then they are executed before the
Pulumi command is run, but only if the command line option '--pre-stage' is used.

Depending on the exit code of the script, the stages Pulumi command will be run (or not).    

See [Exit Codes](#exit-codes) for the values that Yoyo uses.

## Script types

Yoyo is young, so this is a work in progress.  Currently Yoyo supports the following script types:

 * .ps1 - powershell core 
 * .bash - ye ol faithful
 * .python - for the data scientists among you
 * .node - just because, why not?

## Location of pre-stage scripts

Yoyo looks in a specific location to find your scripts. 

The location of the scripts follows this format: 
    
```plaintext
<environment directory>/.yoyo/<environment name>/<stack short name>/pre-stage.[ps1|bash|python|node]
```

The environment directory is _either_ declared in the `Environment.DefaultDirectoryForEnvironment` section of the JSON configuration, in which
case this value is used, or it is the current working directory.  See also [Yoyo Configuration](#yoyo-configuration).

Assuming the default directory for the environment is `g:/src/quickstart/testing` and the environment 
name is `dev`, yoyo will look in the following locations for the pre-stage scripts:

```
g:/src/quickstart/testing/.yoyo/dev/app/pre-stage.ps1
g:/src/quickstart/testing/.yoyo/dev/mssql/pre-stage.ps1
g:/src/quickstart/testing/.yoyo/dev/cluster/pre-stage.ps1
```

# YoYo Configuration

Yoyo configuration is just a plain boring JSON file.  Nothing more.  No fancy DSL, no magic, no Pulumi!  Just a JSON file.  

The configuration file is used to :
1. give short names to each stack definition.
2. tell Yoyo where on your file system the stack resides.
3. tell Yoyo the full name of the stack that should be operated on.

Here's a full example of a configuration - hopefully this is reasonably clear.   

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

## Configuration Parameters

### Name
The name of the environment.  This is used to find the correct environment configuration when running yoyo commands.  It is also used to find the correct pre-stage scripts.

### Environment
The environment configuration.  This is used to set the default directory for the environment.  It is also used to set the subscription name for the environment.  The subscription name is not used by Yoyo, but is useful for documentation purposes.

### Stacks
The list of stacks that are part of the environment.  Each stack has a short name, a directory path and a full stack name.  The short name is used to identify the stack in yoyo commands.  The directory path is used to find the stack on the file system.  The full stack name is used to identify the stack in pulumi commands.  The depends on list is used to determine the order in which the stacks should be operated on.  This is useful for example when bringing up a stack hierarchy, where one stack depends on another.

## Parameters to pre-stage scripts

No parameters are passed to the pre-stage scripts, but they can use environment variables to get information about the stack
that is being operated on.

The following environment variables are set _in addition to every current environment variable being injected into the script process_:

| Variable                   | Description                         | Example                                  |
|----------------------------|-------------------------------------|------------------------------------------|
| YOYO_STAGE                 | The stage name                      | preview, up, destroy                     |
| YOYO_STACK_SHORT_NAME      | The short name of the stack         | app, mssql, cluster                      |
| YOYO_STACK_FULL_STACK_NAME | The full name of the stack          | test-std-example-app-dev                 |
| YOYO_OPTION_DRYRUN         | True if the command is a dryrun     | True                                     |
| YOYO_OPTION_CONFIG         | The path to the config file         | g:/src/quickstart/testing/my-config.json |
| YOYO_OPTION_FROM_STACK     | The stack to start from             | app, mssql, cluster                      |
| YOYO_OPTION_TO_STACK       | The stack to finish on              | app, mssql, cluster                      |
| YOYO_OPTION_VERBOSE        | True if the command is verbose      | True                                     | 
| YOYO_WORKING_DIRECTORY     | The directory the command is run in | g:/src/quickstart/testing/example-app    |

## Exit Codes

Fair warning: the exit codes are likely to change.  

Exit codes are used to steer further processing of the stack hierarchy.  If a pre-stage script returns a non-zero exit code, then the
command will not proceed to the next stack in the hierarchy.  This is useful for example if you want to stop the command from proceeding
if a certain condition is met.

If the exit code is zero, then the command will **ALSO NOT** proceed to the next stack in the hierarchy.

That is worth repeating: if the exit code is zero, then the command will **ALSO NOT** proceed to the next stack in the hierarchy.

To ensure that the pulumi command IS RUN, the script must return an exit code of exactly: 100.

Weird, but that's how it works today. 
