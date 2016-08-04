-------------------------------------------------------------------------------
-- Title      : I2C controller driven by VIO objects
-- Project    : 
-------------------------------------------------------------------------------
-- File       : i2c_vio_ctrl_top.vhd
-- Author     : Wojciech M. Zabolotny wzab01<at>gmail.com
-- License    : PUBLIC DOMAIN
-- Company    : 
-- Created    : 2015-05-03
-- Last update: 2016-08-04
-- Platform   : 
-- Standard   : VHDL'93/02
-------------------------------------------------------------------------------
-- Description:
-- This core allows you to configure different programmable clocks in the AFCK
-- board.
-- The core measures the frequency of clock on the FPGA_CLK1_P(N) differen-
-- tial input.
-- The I2C may control one of five busses, depending on the value written
-- to the Vio_i2c_sel
-- Suggested method of operation:
-- 1) 
-------------------------------------------------------------------------------
-- Copyright (c) 2015 
-------------------------------------------------------------------------------
-- Revisions  :
-- Date        Version  Author  Description
-- 2015-05-03  1.0      wzab    Created
-------------------------------------------------------------------------------

library ieee;
use ieee.std_logic_1164.all;
use ieee.numeric_std.all;
library unisim;
use unisim.vcomponents.all;

entity i2c_j1b_ctrl_top is
  generic (
    NUM_I2CS : integer := 5);
  port (
    clk0_n      : in    std_logic;
    clk0_p      : in    std_logic;
    clk1_n      : in    std_logic;
    clk1_p      : in    std_logic;
    clk2_n      : in    std_logic;
    clk2_p      : in    std_logic;
    clk         : in    std_logic;
    -- Pin needed to enable switch matrix
    clk_updaten : out   std_logic;
    -- Pin needed to enable Si570
    si570_oe    : out   std_logic;
    --rst_p : in    std_logic;
    scl         : inout std_logic_vector(NUM_I2CS-1 downto 0);
    sda         : inout std_logic_vector(NUM_I2CS-1 downto 0);
    uart_rxd    : out   std_logic;
    uart_txd    : in    std_logic
    );

end entity i2c_j1b_ctrl_top;

architecture beh of i2c_j1b_ctrl_top is

  signal frq0_in  : std_logic;
  signal clk_frq0 : std_logic_vector(31 downto 0);
  signal frq1_in  : std_logic;
  signal clk_frq1 : std_logic_vector(31 downto 0);
  signal frq2_in  : std_logic;
  signal clk_frq2 : std_logic_vector(31 downto 0);
  signal lpbck0, lpbck1, lpbck2, lpbck3 : std_logic_vector(31 downto 0);


begin

  si570_oe    <= '1';
  clk_updaten <= '1';

  ibufgds0 : IBUFDS port map(
    i  => clk0_p,
    ib => clk0_n,
    --ceb => '0',
    o  => frq0_in
    );
  ibufgds1 : IBUFDS_GTE2 port map(
    i   => clk1_p,
    ib  => clk1_n,
    ceb => '0',
    o   => frq1_in
    );
  ibufgds2 : IBUFDS_GTE2 port map(
    i   => clk2_p,
    ib  => clk2_n,
    ceb => '0',
    o   => frq2_in
    );

  -- J1B processor for I2C control
  j1_env_1 : entity work.j1_env
    generic map (
      NUM_I2CS => NUM_I2CS)
    port map (
      clk     => clk,                   -- boot_clk
      rst_n   => '1',                   -- was sys_rst(0), but didnt work!
      scl     => scl,
      sda     => sda,
      uart_tx => uart_rxd,
      uart_rx => uart_txd,
      clk_0   => frq0_in,
      clk_1   => frq1_in,
      clk_2   => frq2_in,
      out0    => lpbck0,
      out1    => lpbck1,
      out2    => lpbck2,
      out3    => lpbck3,
      inp0    => lpbck0,
      inp1    => lpbck1,
      inp2    => lpbck2,
      inp3    => lpbck3
      );

end architecture beh;

