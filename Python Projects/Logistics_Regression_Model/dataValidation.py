# Validating age is number and between a specific range
def IsAgeNumeric(ageData):
    if ageData.isnumeric() and int(ageData) >= 1 and int(ageData) <= 99:
        return True
    else: 
        return False
    
def AgeRangeValid(ageRange1, ageRange2):
    if(ageRange1 < ageRange2):
        return True
    else:
        return False

# Validating BMD is a float between 0 and 1
def IsBMD_Float(bmd):
    try:
        bmd_val = float(bmd)
        if 0.0 <= bmd_val <= 1.0:
            return True
        else:
            return False
    except ValueError:
        return False
    
def IsValidWeightOrHeight(weight_kg):
    try:
        weight_kg = float(weight_kg)
        return True
    except ValueError:
        return False
        
                
# -*- coding: utf-8 -*-
"""
Created on Fri Oct  3 18:33:26 2025

@author: monty
"""

