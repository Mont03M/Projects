# Logistic Model - Predicting Fractures Based on Bone Mineral Density (BMD)

This program is written in Python and utilizes a GUI interface to accept user inputs and estimate the likelihood of fracture risk in patients. Predictions are based on age, sex, weight (kg), height (cm), and bone mineral density (BMD).

Once the inputs are entered, the data is fed into a logistic regression model to generate predictions. The model evaluates fracture risk across a specified age range (age 1 < age 2) and is highly dependent on BMD values.

## Model Overview

The logistic regression model is used to solve a binary classification problem. Logistic regression is well-suited for classification tasks where the outcome is categorical.

In this case, the model predicts whether a patient is likely to experience a fracture based on several predictors, including age, sex, weight, height, and BMD. The target variable is the presence or absence of a fracture.

Additionally, multiple evaluation metrics are used to assess the model’s performance and ensure accuracy and reliability in predictions.

## Dataset Information

File: Fractures.xls<br>
Columns: ID, age, sex, fracture, weight_kg, height_cm, medication, waiting_time, bmd<br>
Total Entries: 170<br>

## Steps to Running Logistic Model GUI:

1. Create a python virtual environment (optional): python -m venv <env_name> <br>
2. Activate the environment: pip install -r requirements.txt<br>
   Alternatively, in a Python IDE or notebook: !pip install -r requirements.txt<br>
   Or: %pip install -r requirements.txt<br>
3. Ensure requirements.txt is installed in the correct environment used by your IDE or kernel. <br>
4. Make sure requirements.txt, model.py, and model_results.py are in the same directory. <br>
