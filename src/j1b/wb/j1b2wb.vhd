-------------------------------------------------------------------------------
-- Title      : j1b2wb
-- Project    : 
-------------------------------------------------------------------------------
-- File       : j1b2wb.vhd
-- Author     : Wojciech M. Zabolotny  <wzab@ise.pw.edu.pl>
-- Company    : Institute of Electronic Systems, Warsaw University of Technology
-- Created    : 2016-07-19
-- Last update: 2016-07-26
-- License    : This is a PUBLIC DOMAIN code, published under
--              Creative Commons CC0 license
-- Platform   : 
-- Standard   : VHDL'93/02
-------------------------------------------------------------------------------
-- Description: J1B -> WB bridge
-------------------------------------------------------------------------------
-- Copyright (c) 2016 
-------------------------------------------------------------------------------
-- Revisions  :
-- Date        Version  Author  Description
-- 2016-07-19  1.0      WZab    Created
-------------------------------------------------------------------------------





library IEEE;
use IEEE.STD_LOGIC_1164.all;
use ieee.numeric_std.all;
library work;
--use work.txt_util.all;
entity j1b2wb is

  generic (
    ADRWIDTH  : integer := 15;          -- Width of the data window
    DATAWIDTH : integer := 32);

  port (
    ---------------------------------------------------------------------------
    -- J1B Interface
    ---------------------------------------------------------------------------
    -- Clock and Reset
    J1B_CLK      : in  std_logic;
    J1B_ARESETN  : in  std_logic;
    -- J1B I/O bus signals
    J1B_IO_RD    : in  std_logic;
    J1B_IO_WR    : in  std_logic;
    J1B_IO_READY : out std_logic;
    J1B_IO_ADDR  : in  std_logic_vector(15 downto 0);
    J1B_DOUT     : in  std_logic_vector(31 downto 0);
    J1B_DIN      : out std_logic_vector(31 downto 0);
    -- J1B Address decoding signals
    J1B_WB_DATA  : in  std_logic;
    J1B_WB_REGS  : in  std_logic;
    -- Here we have the WB ports
    -- The clock and reset are comming from AXI!
    wb_clk_o     : out std_logic;
    wb_rst_o     : out std_logic;
    -- master_ipb_out - flattened due to Vivado inability to handle user types
    -- in BD
    wb_addr_o    : out std_logic_vector(31 downto 0);
    wb_dat_o     : out std_logic_vector(31 downto 0);
    wb_we_o      : out std_logic;
    wb_sel_o     : out std_logic_vector(3 downto 0);
    wb_stb_o     : out std_logic;
    wb_cyc_o     : out std_logic;
    -- master_ipb_in -  flattened due to Vivado inability to handle user types
    -- in BD
    wb_dat_i     : in  std_logic_vector(31 downto 0);
    wb_err_i     : in  std_logic;  -- Not used in figure 1-2 in specification!
    wb_ack_i     : in  std_logic
    );

end entity j1b2wb;

architecture beh of j1b2wb is


  constant L2NREGS : integer    := 1;
  constant NREGS : integer := 2**L2NREGS;
  type T_J1B_REGS is array (0 to NREGS-1) of std_logic_vector(31 downto 0);
  signal J1B_REGS  : T_J1B_REGS := (others => (others => '0'));

  signal   s_J1B_DIN      :  std_logic_vector(31 downto 0);
  
  impure function a_j1b2wb (
    constant j1b_addr : std_logic_vector(15 downto 0))
    return std_logic_vector is
    variable wb_addr : std_logic_vector(31 downto 0);
  begin  -- function a_axi2wb
    wb_addr                      := (others => '0');
    -- Divide the address by 4 (we use word addresses, not the byte addresses)
    wb_addr(ADRWIDTH-1 downto 0) := J1B_IO_ADDR(ADRWIDTH-1 downto 0);
    wb_addr(31 downto ADRWIDTH)  := J1B_REGS(0)(31 downto ADRWIDTH);
    return wb_addr;
  end function a_j1b2wb;

begin  -- architecture beh

  wb_clk_o <= J1B_CLK;
  wb_rst_o <= not J1B_ARESETN;
  wb_sel_o <= (others => '1');          -- We support only whole word accesses

  -- Process generating the WB signals
  qq : process (J1B_DOUT, J1B_IO_ADDR, J1B_IO_RD, J1B_IO_WR, J1B_REGS,
                J1B_WB_DATA, J1B_WB_REGS, wb_ack_i, wb_dat_i, wb_err_i) is
  begin  -- process qq
    -- Defaults
    wb_stb_o     <= '0';
    wb_addr_o    <= (others => '0');
    wb_dat_o     <= (others => '0');
    wb_we_o      <= '0';
    wb_cyc_o     <= '0';
    s_J1B_DIN     <= (others => '0');
    J1B_IO_READY <= '0';
    if J1B_IO_WR = '1' and J1B_WB_DATA = '1' then
      wb_addr_o <= a_j1b2wb(J1B_IO_ADDR);
      wb_dat_o  <= J1B_DOUT;
      wb_stb_o  <= '1';
      wb_cyc_o  <= '1';
      wb_we_o   <= '1';
      -- Currently we ignore errors!
      if wb_ack_i = '1' or wb_err_i = '1' then
        J1B_IO_READY <= '1';
      else
        J1B_IO_READY <= '0';
      end if;
    elsif J1B_IO_RD = '1' and J1B_WB_DATA = '1' then
      wb_addr_o <= a_j1b2wb(J1B_IO_ADDR);
      wb_dat_o  <= J1B_DOUT;
      wb_stb_o  <= '1';
      wb_cyc_o  <= '1';
      wb_we_o   <= '0';
      -- Currently we ignore errors!
      if wb_ack_i = '1' or wb_err_i = '1' then
        J1B_IO_READY <= '1';
      else
        J1B_IO_READY <= '0';
      end if;
      s_J1B_DIN <= wb_dat_i;
    elsif J1B_IO_RD = '1' and J1B_WB_REGS = '1' then
      -- Asynchronous read
      s_J1B_DIN   <= J1B_REGS(to_integer(unsigned(J1B_IO_ADDR(L2NREGS-1 downto 0))));
      J1B_IO_READY <= '1';
    elsif J1B_IO_WR = '1' and J1B_WB_REGS = '1' then
      -- Write handled in synchronous process
       J1B_IO_READY <= '1';
    end if;
  end process qq;

  -- Process handling writing to the J1B2WB control registers
  s1 : process (J1B_ARESETN, J1B_CLK) is
  begin  -- process s1
    if J1B_ARESETN = '0' then           -- asynchronous reset (active low)
      J1B_REGS <= (others => (others => '0'));
    elsif J1B_CLK'event and J1B_CLK = '1' then  -- rising clock edge
      J1B_DIN <= s_J1B_DIN;
      if J1B_IO_WR = '1' and J1B_WB_REGS = '1' then
        J1B_REGS(to_integer(unsigned(J1B_IO_ADDR(L2NREGS-1 downto 0)))) <= J1B_DOUT;
      end if;
    end if;
  end process s1;

end architecture beh;
