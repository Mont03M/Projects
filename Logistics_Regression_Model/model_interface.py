# imports    
import model as model
import dataValidation as validator
import tkinter as tk
from tkinter import ttk
from tkinter import messagebox

def submit_form():
    """Handles the form submission and displays results."""

    # clear results
    result_textbox.delete("1.0", tk.END)
    result_textbox.configure(state="normal") # enable text box editing

    # assign to variables
    age1 = startAge_entry.get()
    age2 = endAge_entry.get()
    weight_kg = weight_kg_entry.get()
    height_cm = height_cm_entry.get()
    bmd = bmd_entry.get()
    M_F = gender_var.get()
    
    # gender
    genderValue = 0 if M_F == "M" else 1

    #----- validate inputs -----
    
    # form filled out
    if not age1 or not age2 or not bmd or not weight_kg or not height_cm or not bmd or not M_F:
        messagebox.showwarning("Input Error", "Please fill in all fields.")
        return

    # ages numerical
    if not validator.IsAgeNumeric(age1):
        messagebox.showerror("From Age Input Error!", "From Age must be between 1-99 and a number\n")
        startAge_entry.delete(0, tk.END)
        return
    
    if not validator.IsAgeNumeric(age2):
        messagebox.showerror("To Age Input Error!", "To Age must be between 1-99 and a number\n")
        endAge_entry.delete(0, tk.END)
        return
    
    
    if not validator.IsValidWeightOrHeight(weight_kg):
        messagebox.showerror("Weight Input Error!", "Weight must be a number\n")
        weight_kg_entry.delete(0, tk.END)
        return
    
    if not validator.IsValidWeightOrHeight(height_cm):
        messagebox.showerror("Height Input Error!", "Height must be a number\n")
        height_cm_entry.delete(0, tk.END)
        return
        
    # bmd float
    if not validator.IsBMD_Float(bmd):
        messagebox.showerror("BMD Input Error!", "BMD input must be between 0-1")
        bmd_entry.delete(0, tk.END)
        return
    
    if not validator.AgeRangeValid(age1, age2):
        messagebox.showerror("Age Range Error!", "From Age must be less then To Age...")
        startAge_entry.delete(0, tk.END)
        endAge_entry.delete(0, tk.END)
        return
    
    # ----- Model -----
    model_logistic.LoadPatients(int(age1), int(age2), model_logistic.Normalize(float(weight_kg)), 
                                model_logistic.Normalize(float(height_cm)), float(bmd), genderValue, True) # load values
    model_logistic.Predict() # predict results
    model_logistic.GenerateModelResults() # generate and store results
    result_textbox.insert(tk.END, model_logistic.resultString, "green") # show results
    result_textbox.configure(state="disabled") # disable text box editing
    model_logistic.Reset() # reset form inputs
    
    # reset form inputs on gui
    startAge_entry.delete(0, tk.END)
    endAge_entry.delete(0, tk.END)
    weight_kg_entry.delete(0, tk.END)
    height_cm_entry.delete(0, tk.END)
    bmd_entry.delete(0, tk.END)
    
    
# ----- Form Fields -----
def create_field(label_text, row, label_col, entry_col, entry_var):
    ttk.Label(main_frame, text=label_text, style="Custom.TLabel").grid(row=row, column=label_col, sticky="e", padx=10, pady=10)
    entry = ttk.Entry(main_frame, textvariable=entry_var, font=entry_font, width=15)
    entry.grid(row=row, column=entry_col, padx=10, pady=10, sticky="w")
    return entry

# ----- Create Radio Field -----
def create_radioField(label_text, row, label_col, entry_col, entry_var):
    # Label 
    tk.Label(main_frame, text=label_text, bg="#708090", fg="white", font=label_font).grid(
        row=row, column=label_col, sticky="e", padx=10, pady=10
    )

    # Frame to hold the radio buttons
    radio_frame = tk.Frame(main_frame, bg="#708090")
    radio_frame.grid(row=row, column=entry_col, sticky="w", padx=10, pady=10)

    # Male radio button (tk.Radiobutton with circular indicator)
    male_radio = tk.Radiobutton(
        radio_frame,
        text="Male",
        variable=entry_var,
        value="M",
        bg="#708090",
        fg="black",
        font=label_font,
        selectcolor="#89c4c4",
        activebackground="#708090",
        activeforeground="white"
    )
    male_radio.pack(side="left", padx=5)

    # Female radio button
    female_radio = tk.Radiobutton(
        radio_frame,
        text="Female",
        variable=entry_var,
        value="F",
        bg="#708090",
        fg="black",
        font=label_font,
        selectcolor="#89c4c4",
        activebackground="#708090",
        activeforeground="white"
    )
    female_radio.pack(side="left", padx=10)

    return radio_frame


if __name__ == "__main__":

    # model
    model_logistic = model.Model('logistic_model.pkl')
    
    # ----- GUI -----
    
    # Create the main window
    root = tk.Tk()
    root.title("Logistic Model BMD Predictor")
    root.geometry("800x550")
    root.resizable(False, False)
    root.configure(bg="#708090")  # Background color
    
    # ----- Fonts and Styles -----
    label_font = ("Helvetica", 10)
    entry_font = ("Helvetica", 12)
    button_font = ("Helvetica", 12, "bold")
    
    # ----- Style labels -----
    style = ttk.Style()
    style.theme_use("default") 
    
    style.configure("Custom.TLabel",
                    background="#708090",  # Match main_frame bg
                    foreground="white",
                    font=label_font)
    
    # ----- Style submit button -----
    style.configure("Custom.TButton")
    
    style.map("Custom.TButton", 
              background=[
                  ("active", "#b2d8d8"),
                  ("pressed", "#99cccc")
                  ])
    
    # ----- Main Frame -----
    main_frame = tk.Frame(root, bg="#708090", pady=20)
    main_frame.pack(side="top", anchor="center", pady=0)
    main_frame.columnconfigure(0, weight=1)
    main_frame.columnconfigure(1, weight=1)
    main_frame.columnconfigure(2, weight=1)
    
    
    # Variables
    start_age_var = tk.StringVar()
    end_age_var = tk.StringVar()
    weight_kg_var = tk.StringVar()
    height_cm_var = tk.StringVar()
    gender_var = tk.StringVar(value="M")
    bmd_var = tk.StringVar()
    
    # Fields
    startAge_entry = create_field("From Age:", 0, 0, 1, start_age_var)
    endAge_entry = create_field("To Age:", 0, 2, 3, end_age_var)
    weight_kg_entry = create_field("Weight(kg):", 0, 4, 5, weight_kg_var)
    height_cm_entry = create_field("Height(cm):", 1, 0, 1, height_cm_var)
    bmd_entry = create_field("BMD:", 1, 2, 3, bmd_var)
    gender_entry = create_radioField("Gender:", 1, 4, 5, gender_var)
    
    # ----- Submit Button -----
    submit_button = ttk.Button(main_frame, text="Submit",  command=submit_form, style="Custom.TButton")
    submit_button.grid(row=2, column=0, columnspan=6, pady=20, sticky="n")
    
    # ----- Result Display with Scrollbar -----
    result_frame = ttk.Frame(main_frame)
    result_frame.grid(row=6, column=0, columnspan=6, sticky="nsew")
    
    # Configuring the frame to expand if needed
    main_frame.grid_rowconfigure(4, weight=1)
    main_frame.grid_columnconfigure(1, weight=1)
    
    
    # Text widget for displaying results
    result_textbox = tk.Text(result_frame, height=20, width=95, font=("Courier", 10), wrap="word")
    result_textbox.tag_configure("green", foreground="green")
    result_textbox.configure(state="disabled")
    result_textbox.pack(side="left", fill="both", expand=True)
    
    # Scrollbar
    scrollbar = ttk.Scrollbar(result_frame, orient="vertical", command=result_textbox.yview)
    scrollbar.pack(side="right", fill="y")
    
    # Attaching scrollbar to text widget
    result_textbox.config(yscrollcommand=scrollbar.set)
    
    # Run the Tkinter event loop
    root.mainloop()
    

# -*- coding: utf-8 -*-
"""
Created on Tue Apr 15 14:04:15 2025

@author: monty
"""

