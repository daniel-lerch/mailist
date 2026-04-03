# Mailist server configuration

Configuration can be set as enviroment variables or by creating a custom config file.
I recommend to use environment variables and will explain them in the following sections.
However, if you prefer a config file, copy the default [appsettings.json](../server/Mailist/appsettings.json), edit it as required, and mount it at `/app/appsettings.json`.

## List of config properties

### Database

#### `Database__ConnectionString`

Contains all important information to connect to a MariaDB/MySQL database. See [Connection Strings](https://learn.microsoft.com/en-us/ef/core/miscellaneous/connection-strings?tabs=dotnet-core-cli) for more information.

Default: `Server=localhost;Port=3306;Database=mailist;User=root;Password=root;`

#### `Database__MigrateOnStartup`

Automatically migrates the database on application startup to the latest migration. If you disable this feature, you must run `./Mailist database migrate` each time after updating Mailist.

Default: `true`

### Hosting

#### `Hosting__PathBase`

Configure Mailists's URI path part if it is not hosted on its own (sub)domain. Your reverse proxy should not rewrite the request URI.

Example: `/mailist`

#### `Hosting__AllowProxies`

Enable this if you run Mailist behind a reverse proxy like NGINX. Make sure Mailist is only accessible via reverse proxy via network configuration, firewall, etc.

Default: `false`

### JWT

#### `Jwt__Issuer`

The issuer (`iss`) claim of issued tokens. Use the public URL of your Mailist backend.

Example: `https://mailist.example.org`

#### `Jwt__Audience`

The audience (`aud`) claim of issued tokens. Use the public URL of your Mailist backend.

Example: `https://mailist.example.org`

#### `Jwt__SigningKey`

A 256‑bit signing key represented as a hex string (exactly 64 hex characters).

Example: `3f9a1b2c...` (64 hex chars)

Generate a secure 256‑bit hex key with OpenSSL:

`openssl rand -hex 32`

Or with PowerShell:

`[System.BitConverter]::ToString([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))`

### ChurchTools

#### `ChurchTools__Host`

The domain of your ChurchTools instance.

Example: `demo.church.tools`

#### `ChurchTools__LoginToken`

The login token of your service account for Mailist. Used to authenticate with the ChurchTools API. This should be a 256 chars long alphanumeric text without special chars.

Example: `gMNSXgU8j4pUlYz1RZwpJMsG0uwM70CmQcJihDaQ4TjorKKQBllnQV6eMDPa7QAT8BxvzzPUN7VTmitRIZtXNK8kBUhGcXEw0eJke8xWKNcho28VegF7dXBbSe1A4YKfn3FZysxSbe7A3ZmSrtLfqIRYagmkaU9TvWgo9iTGRbtS4tkRrsUj36gGjIGWL4UALNJaCj8syxanjxr7oPbXdbaRdx8uNiOYXmLwMx0CQHs9yT2hA8O1pnPcM4XuACTl`

#### `ChurchTools__SyncIntervalInMinutes`

The interval in minutes at which Mailist synchronizes people, groups and memberships from ChurchTools.

Default: `5.0` (5 minutes)

### Email delivery

#### `EmailDelivery__Enable`

Enable or disable email delivery via STMP.

Default: `false`

#### `EmailDelivery__SenderName`

The name that will appear as the sender of the emails. You may want to set this to the name of your church or organization.

Default: `Mailist`

#### `EmailDelivery__SenderAddress`

The email address that will appear as the sender of the emails.

Example: `noreply@example.org`

#### `EmailDelivery__SmtpHost`

The SMTP server host used for sending emails.

Default: `smtp.strato.de`

#### `EmailDelivery__SmtpPort`

The port used to connect to the SMTP server.

Default: `465`

#### `EmailDelivery__SmtpUseSsl`

Indicates whether to use SSL for the SMTP connection.

Default: `true`

#### `EmailDelivery__SmtpUsername`

The username used to authenticate with the SMTP server.

Example: `noreply@example.org`

#### `EmailDelivery__SmtpPassword`

The password used to authenticate with the SMTP server.

Example: `dehxo3ql5uke`

### Email relay

#### `EmailRelay__Enable`

Enable or disable email relay. Mailist will then periodically check for emails and process them according to the distribution lists you created. Email delivery must be enabled for this option to have an effect.

Default: `false`

#### `EmailRelay__ImapHost`

The IMAP server host used for retrieving emails.

Default: `imap.strato.de`

#### `EmailRelay__ImapPort`

The port used to connect to the IMAP server.

Default: `993`

#### `EmailRelay__ImapUseSsl`

Indicates whether to use SSL for the IMAP connection.

Default: `true`

#### `EmailRelay__ImapUsername`

The username used to authenticate with the IMAP server. This should be a catchall inbox that receives all emails on that domain for which no mailbox exists.

Example: `catchall@example.org`

#### `EmailRelay__ImapPassword`

The password used to authenticate with the IMAP server.

Example: `dehxo3ql5uke`

#### `EmailRelay__ImapRetentionIntervalInDays`

The number of days to retain emails on the IMAP server before deletion.

Default: `1.0` (24 hours)  
Min value: `0.0` (delete emails immediately after processing)  
Max value: `87600.0` (keep emails for 10 years)

#### `EmailRelay__MaxHeaderSizeInKilobytes`

The maximum size of email headers in kilobytes.

Default: `64` (64 KiB)  
Min value: `16` (16 KiB, a usual email SPF, DKIM and DMARC headers is not much smaller)  
Max value: `1024` (1 MiB headers)

#### `EmailRelay__MaxBodySizeInKilobytes`

The maximum size of email bodies in kilobytes.
You must increase MariaDB's `max_packet_size` if you want to increase this limit.

Default: `12288` (12 MiB)  
Min value: `64` (64 KiB)  
Max value: `131072` (128 MiB, almost no email server will accept such a large message)
