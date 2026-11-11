# RaceDay – Part 3 MVC application

This folder contains the Part 3 extension for the supplied RaceDay Part 2 project.

## Important implementation notes

- `RaceDay.MVC` is the ASP.NET Core MVC front end.
- The MVC project calls the existing `RaceDay.API` endpoints; it does not use Entity Framework or connect to SQL Server.
- The MVC app forwards the API's `.RaceDay.Session` cookie on server-to-server requests, so the API remains responsible for role checks.
- The supplied Part 2 API did not contain an image-upload endpoint or cloud-storage SDK. This package adds `api/images/event-banner` and `api/images/profile-picture` to the Part 2 API and uses Cloudinary for those uploads.
- **POE compliance note:** the written POE specifies Azure Blob Storage. Cloudinary is a different provider. If your lecturer requires literal Azure Blob Storage, replace the Cloudinary upload implementation with Azure Blob Storage before submission. This implementation follows the previously selected Cloudinary approach, not the literal Azure requirement.

## Visual Studio setup

1. Extract the complete project folder.
2. Open `RaceDay.sln` in Visual Studio 2022.
3. Confirm the solution targets .NET 8. Install the .NET 8 SDK if required.
4. Right-click the solution and choose **Restore NuGet Packages**.
5. Open `RaceDay.API/appsettings.json` and confirm the LocalDB connection string.
6. Set up a Cloudinary account. Set these environment variables in the API launch profile or Windows environment:
   - `CLOUDINARY_CLOUD_NAME`
   - `CLOUDINARY_API_KEY`
   - `CLOUDINARY_API_SECRET`
   Or fill the `Cloudinary` values in `RaceDay.API/appsettings.json` for local testing only. Do not commit real credentials.
7. In `RaceDay.MVC/appsettings.json`, set `ApiSettings:BaseUrl` to the API's HTTP address. The supplied API launch profile uses `http://localhost:5003/`.
8. Apply the database migration from Visual Studio Package Manager Console with `RaceDay.API` selected as the Default project:
   `Update-Database`
9. Start the API first. In Visual Studio, right-click `RaceDay.API` > Debug > Start new instance. Confirm Swagger opens at `http://localhost:5003/swagger`.
10. Start the MVC app. Right-click `RaceDay.MVC` > Debug > Start new instance. Browse to its launch URL.
11. Register an Organiser and create an event. Add event categories. Register a Participant in a separate browser session and enter an event. Return to the Organiser session to review enrolments and capture results.
12. Test profile and event image uploads after Cloudinary credentials are configured.

## Main MVC pages

- Public: Home, Events, Event details.
- Account: Register, Login, Logout.
- Organiser: Dashboard, Create/Edit/Delete event, Categories, Enrolments and Results capture.
- Participant: My Enrolments, My Results.
- Shared: Profile view/update.

## Docker

Docker requires a SQL Server instance accessible to the containers and the Cloudinary environment variables. Set `SA_PASSWORD` and the three Cloudinary variables in your shell, then run:

```bash
docker compose build
docker compose up
```

MVC: `http://localhost:8080`

The API uses the database connection string in `docker-compose.yml`. Adjust it for your SQL Server setup. The supplied LocalDB connection string cannot be used from inside a Linux container.

## GitHub and submission checklist

- Push the MVC project, API image-upload changes, Dockerfiles, compose file and updated workflow.
- Make at least 20 meaningful commits as required by the POE; do not create artificial commits just to reach the number.
- Wait for GitHub Actions to pass, then add a screenshot of the green build.
- Record your own unlisted YouTube demonstration and add the URL here.
- Add your AI-use disclosure as required by the POE. Review and understand all generated code before submitting.
