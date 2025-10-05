import numpy as np
import joblib

# Model Class
class Model(object):
    def __init__(self, file):
        self.patients = []
        self.model_lr = joblib.load(file)
        self.model_pred = None
        self.model_results = []
        self.resultString = ""
    
    # Load data into model
    def LoadPatients(self, startRange, endRange, weight_kg, height_cm, bmd, M_F, convertToNpArray):
        'age', 'sex', 'bmd', 'weight_kg', 'height_cm'
        # male or female patients
        for index, age in enumerate(range(startRange, endRange+1)):
            self.patients.append([age, M_F, bmd, weight_kg, height_cm])
        if convertToNpArray and len(self.patients) != 0: # convert to a np array
            self.patients = np.array(self.patients)
        if not self.patients.any(): # list empty ?
            print("List is empty")
            
    #Normalize value        
    def Normalize(self, val):        
        val /= 100
        return val
        
    # generate predictions
    def Predict(self):
        if len(self.patients != 0):
            self.model_pred = self.model_lr.predict(self.patients)
        else:
            print("No predictions made! Patient list is empty...")
    
    # generate and store results
    def GenerateModelResults(self):
        
        # store results
        result_lines = []
        
        # generating results from the list of patients
        for index, col in  enumerate(self.patients):
          patient_age = col[0].astype(int)
          patient_sex = col[1].astype(int)
          patient_bmd = col[2].astype(float)
          patient_weight_kg = col[3].astype(float) * 100
          patient_height_cm = col[4].astype(float) * 100
          
          gender = "Female" if patient_sex else "Male"
          model_result = self.model_pred[index].astype(int) == True
          self.model_results.append([patient_sex, patient_age, patient_bmd, patient_weight_kg, patient_height_cm, model_result])
          
          # line of results
          line = (
          f"{'Gender:':<8}{gender:<6}"
          f"{'Age:':<5}{patient_age:<5}"
          f"{'Weight(kg):':<12}{patient_weight_kg:<7}"
          f"{'Height(cm):':<12}{patient_height_cm:<7}"
          f"{'BMD:':<5}{patient_bmd:<7.3f}"
          f"{'Fracture Risk: ':<5}{str(model_result)}")
          result_lines.append(line)  
        self.resultString = "\n".join(result_lines)
    
    # Reset model results and predictions
    def Reset(self):
        self.patients = []
        self.model_pred = None
        self.model_results = []
        self.resultString = ""



# -*- coding: utf-8 -*-
"""
Created on Fri Oct  3 18:29:29 2025

@author: monty
"""

