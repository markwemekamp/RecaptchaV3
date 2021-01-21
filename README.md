# 🤖 Recaptcha V3 for Umbraco Forms 8+

![CI](https://github.com/markwemekamp/RecaptchaV3/workflows/CI/badge.svg)

This is a fork of [https://github.com/shaishavkarnani/RecaptchaV3](https://github.com/shaishavkarnani/RecaptchaV3)

Adds an Umbraco form fieldtype to your Umbraco 8 installation that validates the submitted form using Google Recaptcha v3.

Changes to the original project:
- Hides the form in the frontend by default
- Allows you to set a score threshold
- The score is posted as part of the entry

## Building a nuget package

```shell 
nuget pack .\RecaptchaV3\RecaptchaV3.nuspec
```
## Building an Umbraco package

```shell 
umbpack pack .\RecaptchaV3\package.xml
```