set_property IOSTANDARD LVCMOS15 [get_ports clk]
set_property PACKAGE_PIN AF6 [get_ports clk]
set_property IOSTANDARD LVCMOS25 [get_ports scl*]
set_property IOSTANDARD LVCMOS25 [get_ports sda*]
#Real SCL and SDA on AFCK
set_property PACKAGE_PIN K19 [get_ports {scl[4]}]
set_property PACKAGE_PIN G19 [get_ports {sda[4]}]

#SCL and SDA for GCLK1 FM-S14 in FMC2 of AFCK
set_property PACKAGE_PIN AD27 [get_ports {scl[3]}]
set_property PACKAGE_PIN AD28 [get_ports {sda[3]}]

#SCL and SDA for GCLK0 FM-S14 in FMC2 of AFCK
set_property PACKAGE_PIN AE28 [get_ports {scl[2]}]
set_property PACKAGE_PIN AF28 [get_ports {sda[2]}]

#SCL and SDA for GCLK1 FM-S14 in FMC1 of AFCK
set_property PACKAGE_PIN E28 [get_ports {scl[1]}]
set_property PACKAGE_PIN D28 [get_ports {sda[1]}]

#SCL and SDA for GCLK0 FM-S14 in FMC1 of AFCK
set_property PACKAGE_PIN D26 [get_ports {scl[0]}]
set_property PACKAGE_PIN C26 [get_ports {sda[0]}]

create_clock -period 50.000 -name clk -waveform {0.000 25.000} [get_ports clk]

# UART connections

set_property PACKAGE_PIN F16 [get_ports uart_txd]
set_property PACKAGE_PIN G12 [get_ports uart_rxd]
set_property IOSTANDARD LVCMOS25 [get_ports uart_txd]
set_property IOSTANDARD LVCMOS25 [get_ports uart_rxd]


set_property PULLUP true [get_ports {sda[4]}]
set_property PULLUP true [get_ports {sda[3]}]
set_property PULLUP true [get_ports {sda[2]}]
set_property PULLUP true [get_ports {sda[1]}]
set_property PULLUP true [get_ports {sda[0]}]
set_property PULLUP true [get_ports {scl[4]}]
set_property PULLUP true [get_ports {scl[3]}]
set_property PULLUP true [get_ports {scl[2]}]
set_property PULLUP true [get_ports {scl[1]}]
set_property PULLUP true [get_ports {scl[0]}]


set_property PACKAGE_PIN AG10 [get_ports clk0_p]
set_property PACKAGE_PIN E8 [get_ports clk1_p]
set_property PACKAGE_PIN G8 [get_ports clk2_p]

# C8 - MGTREFCLK0P_118   should go to LINK23_CLK
# E8 - MGTREFCLK1P_118 - may go to FMC1_GBTCLK0_M2C
# G8 - MGTREFCLK0P_117 - may go to FMC2_GBTCLK0_M2C
# J8 - MGTREFCLK1P_117 - should go to LINK01_CLK

set_property IOSTANDARD DIFF_SSTL15 [get_ports clk0_p]



set_property PACKAGE_PIN AE16 [get_ports clk_updaten]
set_property PACKAGE_PIN Y20 [get_ports si570_oe]
set_property IOSTANDARD LVCMOS18 [get_ports clk_updaten]
set_property IOSTANDARD LVCMOS25 [get_ports si570_oe]
set_property C_CLK_INPUT_FREQ_HZ 300000000 [get_debug_cores dbg_hub]
set_property C_ENABLE_CLK_DIVIDER false [get_debug_cores dbg_hub]
set_property C_USER_SCAN_CHAIN 1 [get_debug_cores dbg_hub]
connect_debug_port dbg_hub/clk [get_nets clk_IBUF_BUFG]
