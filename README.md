# AFCK_J1B_FORTH
This repository contains HDL anf forth sources for the Forth based system for AFCK board initialization and diagnostics. 
Significant of this project is "swapforth" and J1B Forth CPU developed by James Bowman (https://github.com/jamesbowman/swapforth)

I have ported J1B to VHDL (in https://github.com/wzab/swapforth ) and added the functionality to dump the porgram/data memory
after compilation of additional commands.

The project reuses also the I2C controller available from OpenCores.

# Usage

Initialization of communication

    AFCK_i2c_init

Setting of the Si57x clock to 125MHz

    4 bus_sel 125000000 Si57x_set_frq

Setting of the clock 0 in FM-S14 in FMC1 to 130MHz

    0 bus_sel 130000000 FMS14Q_SetFrq
    
Switching the clock matrix (input 15 routed to output 4)

    4 bus_sel 15 4 ClkMtx_set_out
