# imports
from sklearn.linear_model import LogisticRegression
from sklearn.preprocessing import OneHotEncoder
from sklearn.model_selection import train_test_split
from sklearn.metrics import roc_curve, roc_auc_score
import matplotlib.pyplot as plt
import pandas as pd
import joblib

# ======== pre-processing dataset ================= #

# loading dataset
df = pd.read_excel("data/Fractures.xls")

print(f"Summarizing the first 5 entries of the dataset\n {df.head(5).to_string()}\n")

print(f"Printing the columns of the dataset\n {df.columns}\n")

# creating one hot encoder to convert categorical variables into a numerical format 
ohe = OneHotEncoder(sparse_output=False, handle_unknown='ignore')

# columns to convert using one-hot-encoding
# encoding categorical dataset into a binary repersentation
encode_fracture_col = ohe.fit_transform(df.copy()[['fracture']])
encode_sex_col = ohe.fit_transform(df.copy()[['sex']])

# setting new values into the dataframe
df['fracture'] = encode_fracture_col # 0 = no fracture, 1 = fracture
df['sex'] = encode_sex_col # 1 = female, 0 = male

df['weight_kg'] /= 100 # normalizing 
df["height_cm"] /= 100 # normalizing

# predictors
predictors = ['age', 'sex', 'bmd', 'weight_kg', 'height_cm']

# model variables 
X = df[predictors].to_numpy()
y = df['fracture'].to_numpy()

# ======================= creating and trainning the model logistic regression ============================== 

# creating train and tests datasets 
X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.20, shuffle=True, random_state=None)

 # creating the model and fitting the dataset
model_lr = LogisticRegression()
model_lr.fit(X_train, y_train, sample_weight=True)

# evaluating model
acc_train = model_lr.score(X_train, y_train)
acc_test = model_lr.score(X_test, y_test)

# results
print("The accuracy trainning score of the model: {:.4f}\n".format(acc_train))
print("The accuracy testing score of the model: {:.4f}\n".format(acc_test))


# ====================== calculating the fpr and tpr for all thresholds of the classification (fracture or no fracture) ===============

# predictions
y_pred = model_lr.predict_proba(X_test)
preds = y_pred[:, 1]


lr_auc = roc_auc_score(y_test, preds)


print('Logistic Regression Prediction: AUROC = %.3f' % (lr_auc))


fpr, tpr, threshold = roc_curve(y_test, preds)


# ====================== creating receiver operating characteristics (ROC) plot =============================

plt.title('Receiver Operating Characteristic')
plt.plot(fpr, tpr, marker='.', linestyle='--', label='Logistic Regression (AUROC = %0.3f)' % lr_auc)
plt.legend(loc = 'lower right')
plt.plot([0, 1], [0, 1], 'r--')
plt.xlim([-0.1, 1])
plt.ylim([0, 1])
plt.ylabel(['True Positive Rate'])
plt.xlabel(['False Positive Rate'])


plt.show()


# === saving the model =================
joblib.dump(model_lr, 'logistic_model.pkl')


# -*- coding: utf-8 -*-
"""
Created on Tue Apr 15 12:06:50 2025

@author: monty
"""

