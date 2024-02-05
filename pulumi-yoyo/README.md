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

You can use Yoyo to bring the whole lot up in one go.

```bash
yoyo up

Would run command: pulumi up --skip-preview -s test-std-cluster-dev --non-interactive
Would run command: pulumi up --skip-preview -s test-std-example-mssql-dev --non-interactive
Would run command: pulumi up --skip-preview -s test-std-example-app-dev --non-interactive
```

TODO: You can also use Yoyo to bring up just the database

```bash
yoyo up -to database
```

TODO: Or just the cluster

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

Lets imagine that I do not want to destroy the cluster, instead I just want to shut it down.  This saves me money, and also time - I don't pay for a running cluster - just the allocated disks.



