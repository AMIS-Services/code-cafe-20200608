# Dev Containers in VS Code

## Introduction
[Dev Containers](https://code.visualstudio.com/docs/remote/containers) in VS Code is a feature, in which you seemlessly integrate docker containers into a VS Code dev/scripting environment. 

For example, you can install and run the compiler of your favorite programming language in a docker container, while keeping the source code on your local system. This is useful when you want to silo different compiler versions or setups, for testing or experimentation. Or if you want to ensure that every team member uses the same setup, ie for setup distribution.

This workshop will showcase an full-fledged setup, with a primary environment for FSharp development, a peripheral service of DynamoDB, with all required VS Code plugins installed and having all extraneous settings configured.


Dev-containers provide the following benefits:
- Environment standardization, an environment configuration is easily distributed (ie to new team-members) because it's all text files;
- The user requires little instruction to setup and start the environment;
- The user may have multiple environments on his system and can easily switch between them;
- Production parity - ie the differences between dev/test/prod environments can be made smaller.


## Pre Requisites

The following must be installed on your Windows machine in order to work with Dev Containers:
- Docker Desktop
- VS Code (latest version)

For other OS's and additional information check [Getting Started](https://code.visualstudio.com/docs/remote/containers#_getting-started).

Note:
- Docker Desktop requires Windows Feature "Hyper-V" to be turned on;
- Docker Running in a Virtual Machine is not in scope for this instruction;
  

## My First Dev Container

The quickest way to install and start a Dev Container is by clicking the Green button / symbol in the left lower corner in VS Code and select _Try a Sample.._. See [Quick Start](https://code.visualstudio.com/docs/remote/containers#_quick-start-try-a-dev-container).

The first time you open any Dev Container in VS Code, the environment will download, install and startup the configuration. This may take a while the first time. If you see an Error, sometimes clicking Retry will make it go away.

When starting a Dev Container, VS Code signals this with the message ``Starting with Dev Container``. If you double click on the message-text, a terminal will open and show logging output.


## Demo a Non-trivial Dev Container Environment
This workshop showcases a non-trivial setup, which required quite some research to develop. It demonstrates the benefits of Dev Containers, by constructing a setup with:
- Dotnet Core 3.1 - F# Compiler Container;
- DynamoDB Container;
- Bridged networking between both containers;
- Installs required F# plugins into VS Code;
- Configuration of an environment variable, in order to run the F# scripting engine in version 5-preview mode, rather than version 4.7.


### Installation
To install and start the setup, press the little green button in the left corner of VS Code and select "Remote-Containers: Open Folder in Container"

Select the folder with the setup, which is the parent-folder of ``.devcontainer``.

Then, everything will be downloaded, installed and the setup will be started.

This particular environment is fully started when the left bar in VS Code shows a white version of the [FSharp logo](https://nl.wikipedia.org/wiki/F%E2%99%AF#/media/Bestand:Fsharp_logo.png) selected and project "app" opened in the left panel. If the initialization hangs, you can try to continue with the next section, or restart VS Code.


### Running project "app"
Running project "app" is easy. Click the "Play" button on the left bar in VS Code, which  looks like a triangle with a little bug on it. If you then click on the little green triangle at the top of the left panel, the app will be build and run. 

The output panel will become visible at the bottom of VS Code and the "Debug Console" tab will show various messages. 

The stdout of running "app" is written to the "Terminal" tab which you must click to view.


### Running F# Scripts
F# can be used as a compiled language as shown in the previous section. But F# can also be used as a strong typed scripting language. In this part we will run scripts in the Dev Container. Click on the files-button in VS Code (top button in the left bar).

Navigate to folder "test-scripts" and open file "Test Dynamo Connection.fsx".

For the workshop this script is build-up in steps. In order to have the best user-experience, it is best to run each step seperately. But you can also run the full script in one go.

You can run selected text as follows. Select the script for Step 1, ie. make a text-selection with your mouse, then press Alt+Enter.

At the bottom, the FSI (Fsharp Interactive) output is send to the "Terminal" tab. First it repeats the text which was selected, then outputs the result.

You can run subsequent steps in the same way.

If the FSI becomes stale, you can destroy it by clicking the trashcan symbol in the "Terminal" tab and run everything from the start.

If you see no errors after running the complete script, the connectivety with the DynamoDB has succeeded. 

Additionally, you can check the network configuration as follows: 
- If you ``ping dynamodb-local`` in _CMD.EXE_ on your local machine, you'll see it is unreachable.
- If you click on the __+__ symbol in "Terminal" tab in the output panel, it will start a new bash shell. If you ``ping dynamodb-local`` in bash, you will see it *is* reachable. 

This network configuration is chosen, not mandatory.


## Create a Non-trivial setup with Dev Container
This section provides a tutorial to create the setup for this workshop's showcase. This may also be used to create setups with other programming languages like C#, Java, Python etc.

### Download the blueprint for the main Dev Container

This showcase used the Dev Container of [F# (.NET Core 3.1)](https://github.com/microsoft/vscode-dev-containers/tree/master/containers/dotnetcore-3.1-fsharp). This can be retrieved by cloning the [repository](https://github.com/microsoft/vscode-dev-containers), or download as Zip. Then copy the requred folder from the repository to the location where you want to create the setup, and rename this folder to ``MySetup``. Note that we're _not_ following the instructions supplied in the folder.

The downloaded repository also provides blueprints for a variaty of other programming languages, which are found in folder [containers](https://github.com/microsoft/vscode-dev-containers/tree/master/containers/).

### Contents of the MySetup folder 

Navigate to the folder ``MySetup``. Two sub-folders are important to create the setup:
- ``.devcontainer`` - to configure main environment, peripheral services and VS Code plugins to download
- ``.vscode`` - to configure VS Code for this setup

### Configuring the Containers
The folder ``.devcontainer`` contains two files:
- ``devcontainer.json`` - the entry point of Dev Container configuration
- ``Dockerfile`` - instructions to build the Dev Container for the main environment

The configuration from the downloaded repository only works with the Dockerfile. But we want to use a peripheral service in an additional container. We need two containers, one with the main development environment, and a second with DynamoDB.

To configure multiple containers, we use the [docker compose](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/multi-container-microservice-net-applications/multi-container-applications-docker-compose) technology. 

For this showcase, the yaml file ``docker-compose.yml`` was created and added to the ``.devcontainer`` folder with the following contents. Note comments explain the settings below:
 
```yaml
# docker-compose.yml
version: "3.3"

services:
    # the container "dynamodb-local" is created from an image
    dynamodb-local:
        image: amazon/dynamodb-local
        hostname: dynamodb
        networks:
          # both containers are added to this network
          - fsharp-dynamodb-network 

    # this is the Dev Container, which just points to the Dockerfile we already have, for creation
    fsharp-devcontainer:
        build:  
            context: .              # current folder 
            dockerfile: Dockerfile  # create from Dockerfile
        hostname: fsharp-devcontainer

        #   This command prevents the dev container from shutting down, which is default behavior
        command: /bin/sh -c "while sleep 1000; do :; done"

        volumes:
          # Mounts the project folder to '/workspace'. The target path inside the container
          # should match what your application expects. In this case, the compose file is
          # in a sub-folder, so we will mount '..'. You would then reference this path as the
          # 'workspaceFolder' in '.devcontainer/devcontainer.json' so VS Code starts here.
          - ..:/workspace:cached
        user: vscode
        depends_on: 
            - dynamodb-local
        networks:
          # both containers are added to this network
          - fsharp-dynamodb-network

networks:
  # both containers were added to this network, here's the configuration
  # by choosing bridged networking, both containers can resolve eachother
  # without exposing ports to the host system.
  fsharp-dynamodb-network:
    driver: bridge
```


No changes are made to ``Dockerfile``. 


The ``devcontainer.json`` is modified to look as follows. Note comments explain the settings below:

```json
{
    "name": "F# (.NET Core 3.1)",

    //  use docker-compose with the yaml file we created, the setting "Dockerfile" must be removed
    "dockerComposeFile": "docker-compose.yml",  
    
    // Now we have two containers in our setup, which one is the Dev Container?
    // We add the configuration of the Dev Container's service name.
	"service": "fsharp-devcontainer",   


	"settings": {
		"terminal.integrated.shell.linux": "/bin/bash",
		"FSharp.useSdkScripts":true
	},

    // This part is unchanged for the showcase, but note, 
    // here you configure the VS Code's plugins which should be installed for your setup.
	"extensions": [
		"Ionide.Ionide-fsharp", 
		"ms-dotnettools.csharp"
	],

    //  These settings must be added
	"remoteUser": "vscode",
	"workspaceFolder": "/workspace",
	"shutdownAction": "stopCompose"
}
```

### Configuring VS Code

The VS Code configuration is found in folder ``.vscode``.

For the showcase only ``settings.json`` was changed. Note comments explain the settings:

```json
{
  "razor.disabled": true,
  "FSharp.fsiExtraParameters": [
    //  extra parameter to run the FSI in FSharp 5-preview mode. 
    //  this makes the #r "nuget:...." lines work in the fsx script.
    //  this preview mode is provided with the FSharp 4.7 installation
    "--langversion:preview" 
  ]
}
```

### Cleanup and add content

After setup configuration, the remaining work is to clean-up the ``MySetup`` folder and add the content you would like to distribute.

You could add an example project if you are creating a standard development environment. Or you could add the contents of a workshop, like this one.

### Distribute

To distribute a setup, you can Zip it. Make sure you check the contents of the resulting Zip-file, because the Windows function "Send to Compressed Folder" may be configured to skip the ``.vscode`` folder.
