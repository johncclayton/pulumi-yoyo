# Pulumi YOYO

A wrapper around your existing Pulumi CLI (and Automation API) that helps you manage a hierarchy of
Pulumi Stacks. 

## Install

dotnet tool install --global Pulumi.Yoyo

## Usage

There is more information in the docs about how to set up a project file, but lets just assume you have a project that brings 
up a cluster, a database and an application and that this is 3 stacks. 

You can use Yoyo to bring the whole lot up in one go.

```bash
yoyo up
```

You can also use Yoyo to bring up just the database

```bash
yoyo up -to database
```

Or just the cluster

```bash
yoyo up -to cluster
```

Or destroy the whole lot

```bash
yoyo destroy
```

# But my stack is a snowflake, I want to XYZ

You can!

