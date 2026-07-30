# Victor (Server Administrator)

**Goal:** Set up and configure Mailist to get email distribution lists working with his ChurchTools instance.

**Context of use:** Has a Docker host ready and ChurchTools admin access; wants to install and configure Mailist end to end.

**Already knows:** Docker and Docker Compose; ChurchTools administration, including permission management; SMTP and IMAP basics, and how to find the required settings at his email hosting provider.

**Does not know:** How Mailist works internally; why a catchall IMAP account is needed and how email relaying works; how Mailist's filters for recipients and authorized senders work; ChurchTools' ChurchQuery feature, which Mailist uses internally.

**Time available:** About 15 minutes to read the docs and identify what he needs (Docker, database, catchall IMAP account, etc.); aims to have Mailist fully set up in under two hours.

**Never reads:** Internal architecture details such as the job queue; low-level details like which email headers are used when; code examples; contributing guidelines.
