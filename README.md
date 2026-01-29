# Mailist

[![Build and tests](https://github.com/daniel-lerch/mailist/actions/workflows/main.yml/badge.svg)](https://github.com/daniel-lerch/mailist/actions/workflows/main.yml)
[![](https://img.shields.io/docker/pulls/daniellerch/mailist.svg)](https://hub.docker.com/r/daniellerch/mailist)
[![](https://img.shields.io/docker/image-size/daniellerch/mailist/latest.svg)](https://hub.docker.com/r/daniellerch/mailist)

Mailist is a ChurchTools extension that enables you to create email distribution lists without operating an own mailserver and synchronize recipients with ChurchTools.

Mailist was previously named _Korga_ for the german term _**K**irchen **Orga**nisation_ (church organization).
It started as a standalone CMS and soon became an extension platform for ChurchTools when ChurchTools' extensions were not available.

## What can Mailist do?

Mailist makes it possible to send emails to ChurchTools groups via email.
For example, anyone can send an email to _youth@example.org_ and Mailist will forward it to all members of your ChurchTools group _Youth_ with role _Leader_.

## Installation

The only officially supported distribution are Docker containers. An official image is available at [daniellerch/mailist](https://hub.docker.com/r/daniellerch/mailist).
If you are using Docker Compose, take a look our [example compose file](docs/compose.yaml) in the `docs` folder.

See [Mailist server configuration](docs/configuration.md) for a full list of configuration options.

### ChurchTools integration

First you must create a service account which Mailist can use for API access to ChurchTools:

1. Create a person with email address in your ChurchTools instance  
   You might want to define a special status for API users
2. Invite that person to ChurchTools
3. Click on the invitation link and set a password
4. Open https://example.church.tools/api/whoami to look up the ID of the newly created user
5. Open https://example.church.tools/api/person/${ID}/logintoken to look up the long-term access token which will be used like an API key

Steps 4. and 5. can also be performed in the ChurchTools web interface: [Official Documentation](https://hilfe.church.tools/wiki/0/API%20Authentifizierung#logintoken)

> [!WARNING]
> For security reasons it is not recommended to let Mailist use your ChurchTools admin account.

Grant the following permission to Mailist's user:

- Administration > Berechtigungen anpassen `churchcore:administer persons`

After creating and configuring a ChurchTools user for Mailist you can finally configure it via [environment variables](docs/configuration.md#churchtools) in `compose.yaml`.

Do not forget to recreate your container to take these changes into effect.

### Email delivery

Email delivery requires an SMTP server and credentials.
This is given for almost any email inbox.
Depending on your email hosting provider you might need an app password.
Email delivery can be configured via [environment variables](docs/configuration.md#email-delivery) in `compose.yaml`.

### Email relay

Mailist's email relay requires a catchall IMAP inbox.
Such an inbox receives all emails sent to a domain where no matching inbox was found, i.e. `*@example.org`.
It can be configured via [environment variables](docs/configuration.md#email-relay) in `compose.yaml`.

## Contributing

Contributions are highly welcome. Please open an issue before implementing a feature to discuss your plans.

Mailist's source code is split into the backend (located in `server`) and the frontend (located in `webapp`).
The following instructions are written for Windows but generally also apply to Linux development setups.

### Tech Stack

Backend

- ASP.NET Core
- Entity Framework Core
- MySQL / MariaDB
- Docker

Frontend

- Vue 3
- TypeScript
- PrimeVue
- Tailwind CSS

### Development Setup

Backend

- Visual Studio 2026
- .NET SDK 10.0
- EF Core CLI Tools _(e.g. `dotnet tool install -g dotnet-ef`)_
- MySQL or MariaDB _(e.g. from [PSModules](https://github.com/daniel-lerch/psmodules))_

Frontend

- Visual Studio Code
- Vue Language Features (Volar) Extension
- NodeJS 24 LTS

During development the frontend running on the Vue CLI development server will use _http://localhost:10501_ as API endpoint.
That means the backend can be running in Visual Studio with Debugger attached.
