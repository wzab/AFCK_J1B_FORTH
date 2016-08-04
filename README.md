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

    125000000 Si57x_SetFrq

Setting of the clock 0 in FM-S14 in FMC1 to 130MHz

    0 bus_sel 130000000 FMS14Q_SetFrq
    
Switching the clock matrix (input 15 routed to output 4)

    15 4 ClkMtx_SetOut

Switching the clock matrix (output 5 switched off)

    -1 7 ClkMtx_SetOut
    
# Quick start

After you clone the repository, you can build the firmware by running the build.sh script in the main directory.
The project uses the VEXTPROJ environment ( https://github.com/wzab/vextproj ) to create the Vivado project and
to build it (of course Vivado must be installed and available in path ).

After successful compilation, you can upload the bistream to the FPGA and connect to it.
If the USB/UART converter connected to the FPGA UART port in the AFCK board is visible as /dev/ttyUSB0, you
can simply start the localtest_afck script in the forth directory.

The firmware compiled from the cloned repository contains already the swapforth words, so the script loads
only files defining additional words related to the configuration of the AFCK board.

You can use defined words, and create new ones. If something goes wrong, you can reconfigure FPGA to start from the begining.

If you want the FPGA to have all AFCK related words and your own words defined immediately after configuration, read the next section.

# Modifying the initial contents of the memory

The initial contents of the memory is defined by the _src/j1b/prog.vhd_ file. It is created by the _src/j1b/ram\_init.py_ script.
The downloaded version of the script is created from the _src/j1b/nuc\_swapforth.hex_ file. But of course you can use another one.

## How to create the hex file with new words?

The standard nuc.hex file is created by the crossassembler run by gforth. Unfortunately the source files for that crossassembler differ from the files that can by loaded by the sscript like localtest_afck (e.g. they must heve defined 
special headers for each word.)
Therefore I have used another approach. James Bowman has provided his J1B with a wonderful Verilator based testbench.
It runs many times faster then simulation of my VHDL port in ghdl.
This testbench may be used to load the definitions of the new words. *( In fact I often use this testbench to quickly
develop new words without using the real AFCK. It works as long as you do not use the specific hardware )* .
In the original/j1b/verilator directory I have created the _localtest2_ and _localtest3_ files that load the _swapforth_ itself or _swapforth_ and AFCK related words. 
They can be a good starting point for development of own words. You can put your words to the existing files or to the new one
(and add it to the appropriate script).

After your desired words are written and tested, you may want to create the hex file with their definitions. It can be done using the _localtest\_afck\_gen\_prog_ script. *( This script uses the feature that I have added to the James testbench - after reading from the io port 0x2345, the testbench dumps the memory contents to the mem_dump.hex file. This file is also available via a symlink from the _src/j1b_ directory, so you can easily use it to create the new _prog.vhd_ file for synthesis.)*

