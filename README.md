# BulkyBookRay

BulkyBookRay is a project that I had been doing in my spare time. I’m following the [Udemy tutorial](https://www.udemy.com/course/complete-aspnet-core-21-course/) from GitHub user [bhrugen](https://github.com/bhrugen) on creating an ASP.NET Core 6 application using Entity Framework. 

I have created two projects, Abby, which gives me an understanding of how to use Razor pages and BulkyBookWeb, which implements the MVC framework and handles repository patterns and units of work.

After completing the tutorial, I integrated Azure services within the application. I used a CI/CD pipeline that uses different databases and storage containers for different environments (e.g. staging, production).
- **Azure Storage**:  Place to store and read images.
- **Azure Web Service**: Where BulkyBookRay is hosted on. I used two Web Apps for each of the environments.
- **Azure Database**: Created two different databases for each environment.
- **Azure DevOps**:  Used Azure Pipelines to build and deploy the BulkyBookRay application onto Azure Web Service. Also, I have protected sensitive variables of the application using their secrets feature.

The websites of my completed project are linked below (beware that it may take a while for the sites to load):
[https://bulkybookray.azurewebsites.net/](https://bulkybookray.azurewebsites.net/)
[https://bulkybookray-staging.azurewebsites.net/](https://bulkybookray-staging.azurewebsites.net/)

The completed project from bhrugen is [linked here.](https://github.com/bhrugen/Bulky)
