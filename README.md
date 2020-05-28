# OCR-Snipping-Converter
C# application that prompts a user to take a snipping of their screen and perform Optical Character Recognition (OCR) to convert the image to text.

## Description
The application creates a snipping tool that collects the sizes of all active monitors and prompts the user to draw a rectangular area for screenshotting purposes. This snipped image will be passed through an **Optical Character Recognition (OCR)** algorithm (utlizing the IronOCR library https://ironsoftware.com/csharp/ocr/) and return any text available within the image.

## How to run
1. Click on the **Snip image on screen to convert to text** button
2. Press down mouse and draw a rectangular area to process
3. Once complete, the image will be shown on the HUI and the text will be processed and placed under the **Output** field
