# AFCK_J1B_FORTH
This repository contains HDL anf forth sources for the Forth based system for AFCK board initialization and diagnostics. 
Significant of this project is "swapforth" and J1B Forth CPU developed by James Bowman (https://github.com/jamesbowman/swapforth)

I have ported J1B to VHDL (in https://github.com/wzab/swapforth ) and added the functionality to dump the porgram/data memory
after compilation of additional commands.

The project reuses also the I2C controller available from OpenCores.

