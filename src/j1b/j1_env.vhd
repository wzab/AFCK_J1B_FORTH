-------------------------------------------------------------------------------
-- Title      : Testbench for design "j1" and the GHDL simulator
-- Project    : 
-------------------------------------------------------------------------------
-- File       : j1_tb.vhd
-- Author     : Wojciech M. Zabolotny  <wzab01@gmail.com>
-- Company    :
-- License    : BSD License
-- Created    : 2016-07-07
-- Last update: 2016-08-04
-- Platform   : 
-- Standard   : VHDL'93/02
-------------------------------------------------------------------------------
-- Description: 
-------------------------------------------------------------------------------
-- Copyright (c) 2016 
-------------------------------------------------------------------------------
-- Revisions  :
-- Date        Version  Author  Description
-- 2016-07-07  1.0      wzab    Created
-------------------------------------------------------------------------------

library ieee;
use ieee.std_logic_1164.all;
use ieee.numeric_std.all;
use std.textio.all;
library work;
use work.ram_prog.all;
-------------------------------------------------------------------------------

entity j1_env is
  generic (
    NUM_I2CS : integer := 5);
  port (
    clk     : in    std_logic;
    rst_n   : in    std_logic;
    scl     : inout std_logic_vector(NUM_I2CS-1 downto 0);
    sda     : inout std_logic_vector(NUM_I2CS-1 downto 0);
    uart_tx : out   std_logic;
    uart_rx : in    std_logic;
    clk_0   : in    std_logic;
    clk_1   : in    std_logic;
    clk_2   : in    std_logic;
    out0    : out   std_logic_vector(31 downto 0);
    out1    : out   std_logic_vector(31 downto 0);
    out2    : out   std_logic_vector(31 downto 0);
    out3    : out   std_logic_vector(31 downto 0);
    inp0    : in    std_logic_vector(31 downto 0);
    inp1    : in    std_logic_vector(31 downto 0);
    inp2    : in    std_logic_vector(31 downto 0);
    inp3    : in    std_logic_vector(31 downto 0)
    );

end entity j1_env;

-------------------------------------------------------------------------------

architecture test of j1_env is

  -- Constants for address definitions
  -- (Please note, that if you modify them, the Forth procedures
  -- may need readjsutment!

  constant UART_DATA   : integer := 16#1000#;
  constant UART_STATUS : integer := 16#2000#;
  constant FRQ_CNT0    : integer := 16#100#;
  constant FRQ_CNT1    : integer := 16#101#;
  constant FRQ_CNT2    : integer := 16#102#;
  constant OUT0_ADDR   : integer := 16#180#;
  constant OUT1_ADDR   : integer := 16#181#;
  constant OUT2_ADDR   : integer := 16#182#;
  constant OUT3_ADDR   : integer := 16#183#;
  constant INP0_ADDR   : integer := 16#190#;
  constant INP1_ADDR   : integer := 16#191#;
  constant INP2_ADDR   : integer := 16#192#;
  constant INP3_ADDR   : integer := 16#193#;
  constant I2C_BUS_SEL : integer := 16#201#;

  -- Access to the WB controller - two 256-word pages.
  constant JWB_REGS_PAGE : integer := 16#60#;
  constant JWB_DATA_PAGE : integer := 16#61#;


  component j1 is
    generic (
      WIDTH : integer);
    port (
      clk       : in  std_logic;
      resetq    : in  std_logic;
      io_rd     : out std_logic;
      io_wr     : out std_logic;
      io_ready  : in  std_logic;
      mem_addr  : out unsigned(15 downto 0);
      mem_wr    : out std_logic;
      dout      : out std_logic_vector(WIDTH-1 downto 0);
      mem_din   : in  std_logic_vector(WIDTH-1 downto 0);
      io_din    : in  std_logic_vector(WIDTH-1 downto 0);
      code_addr : out unsigned(12 downto 0);
      insn      : in  std_logic_vector(15 downto 0));
  end component j1;

  component j1b2wb is
    generic (
      ADRWIDTH  : integer;
      DATAWIDTH : integer);
    port (
      J1B_CLK      : in  std_logic;
      J1B_ARESETN  : in  std_logic;
      J1B_IO_RD    : in  std_logic;
      J1B_IO_WR    : in  std_logic;
      J1B_IO_READY : out std_logic;
      J1B_IO_ADDR  : in  std_logic_vector(15 downto 0);
      J1B_DOUT     : in  std_logic_vector(31 downto 0);
      J1B_DIN      : out std_logic_vector(31 downto 0);
      J1B_WB_DATA  : in  std_logic;
      J1B_WB_REGS  : in  std_logic;
      wb_clk_o     : out std_logic;
      wb_rst_o     : out std_logic;
      wb_addr_o    : out std_logic_vector(31 downto 0);
      wb_dat_o     : out std_logic_vector(31 downto 0);
      wb_we_o      : out std_logic;
      wb_sel_o     : out std_logic_vector(3 downto 0);
      wb_stb_o     : out std_logic;
      wb_cyc_o     : out std_logic;
      wb_dat_i     : in  std_logic_vector(31 downto 0);
      wb_err_i     : in  std_logic;
      wb_ack_i     : in  std_logic);
  end component j1b2wb;

  component uart is
    generic (
      brg_div : integer);
    port (
      clk         : in  std_logic;
      din         : in  std_logic_vector(7 downto 0);
      dout        : out std_logic_vector(7 downto 0);
      rx_rd       : in  std_logic;
      tx_wr       : in  std_logic;
      nrst        : in  std_logic;
      tx_empty    : out std_logic;
      tx_finished : out std_logic;
      rx_full     : out std_logic;
      fr_err      : out std_logic;
      ovr_err     : out std_logic;
      tx          : out std_logic;
      rx          : in  std_logic);
  end component uart;

  component frq_counter is
    generic (
      CNT_TIME   : integer;
      CNT_LENGTH : integer);
    port (
      ref_clk : in  std_logic;
      rst_p   : in  std_logic;
      frq_in  : in  std_logic;
      frq_out : out std_logic_vector(CNT_LENGTH-1 downto 0));
  end component frq_counter;

-- component ports
  signal uart_rd, uart_wr                 : std_logic;
  signal uart_din, uart_dout              : std_logic_vector(7 downto 0);
  signal uart_dav, uart_ready, uart_empty : std_logic;
  signal code_addr                        : unsigned(12 downto 0);
  signal dout, dout_d                     : std_logic_vector(31 downto 0);
  signal insn                             : std_logic_vector(15 downto 0);
  signal io_din, mem_din                  : std_logic_vector(31 downto 0);
  signal io_rd, io_rd_d                   : std_logic;
  signal io_wr, io_wr_d                   : std_logic;
  signal mem_addr, mem_addr_d             : unsigned(15 downto 0);
  signal io_addr, io_addr_d               : unsigned(15 downto 0);
  signal mem_wr                           : std_logic;
  signal io_ready                         : std_logic;
  signal resetq                           : std_logic                     := '0';
  signal sv_mem_addr                      : std_logic_vector(15 downto 0) := (others => '0');


  -- Internal Wishbone controller and bus
  signal wb_test_dout                 : std_logic_vector(31 downto 0);
  signal wb_addr, wb_addr_d, wb_ready : std_logic;

  signal jwb_data, jwb_regs, jwb_ready : std_logic;
  signal jwb_dout                      : std_logic_vector(31 downto 0);

  signal clk_frq0, clk_frq1, clk_frq2 : std_logic_vector(31 downto 0);

  signal wb_addr_o  : std_logic_vector(31 downto 0);
  signal wb_dat_i   : std_logic_vector(31 downto 0);
  signal wb_dat_o   : std_logic_vector(31 downto 0);
  signal wb_rst_o   : std_logic;
  signal wb_clk_o   : std_logic;
  signal wb_cyc_o   : std_logic;
  signal wb_sel_o   : std_logic_vector(3 downto 0);
  signal wb_stb_o   : std_logic;
  signal wb_we_o    : std_logic;
  signal wb_ack_i   : std_logic;
  signal wb_err_i   : std_logic := '0';
  signal wb_stall_i : std_logic;

  signal wbt_do1_o : std_logic_vector(31 downto 0);
  signal wbt_di1_i : std_logic_vector(31 downto 0);

  signal read_req_1, read_req_2                       : std_logic;
  signal data_valid_1, data_valid_2                   : std_logic;
  signal scl_o, scl_oen, sda_o, sda_oen, scl_i, sda_i : std_logic;

  -- clock
  --signal Clk : std_logic := '1';
  -- Initialization of the memory


  -- Program and data memory in form which can be inferred by Vivado

  shared variable ram : T_RAM_PROG := ram_init;
  signal codeaddr     : unsigned(12 downto 0);
  signal ram_data     : std_logic_vector(31 downto 0);
  signal code_sel     : std_logic;


  -- Reset counter
  signal reset_count : unsigned(31 downto 0) := (others => '0');

  -- Signals for I2C bus selector
  signal i2c_sel_reg : std_logic_vector(7 downto 0) := (others => '0');
  signal i2c_sel     : integer;

  -- Signals for output registers
  signal out0_reg, out1_reg, out2_reg, out3_reg : std_logic_vector(31 downto 0);


begin  -- architecture test

  i2c_sel <= to_integer(unsigned(i2c_sel_reg));
  out0    <= out0_reg;
  out1    <= out1_reg;
  out2    <= out2_reg;
  out3    <= out3_reg;

  P1 : process (clk, rst_n) is
  begin  -- process P1
    if rst_n = '0' then                 -- asynchronous reset (active high)
      reset_count <= (others => '0');
      resetq      <= '0';
    elsif clk'event and clk = '1' then  -- rising clock edge
      if reset_count < 20000000 then
        resetq      <= '0';
        reset_count <= reset_count + 1;
      else
        resetq <= '1';
      end if;
    end if;
  end process P1;

  codeaddr <= '0' & code_addr(12 downto 1);
  -- Program and data memory
  P2a : process (clk) is
  begin  -- process
    if clk'event and clk = '1' then     -- rising clock edge
      ram_data <= ram(to_integer(unsigned(codeaddr)));
    end if;
  end process;

  P2 : process (clk) is
  begin
    if clk'event and clk = '1' then
      code_sel <= code_addr(0);
    end if;
  end process;

  insn <= ram_data(31 downto 16) when code_sel = '1' else ram_data(15 downto 0);

  P2b : process (clk) is
    variable ram_data : std_logic_vector(31 downto 0);
  begin  -- process
    if clk'event and clk = '1' then     -- rising clock edge
      if mem_wr = '1' then
        ram(to_integer(unsigned(mem_addr(14 downto 2)))) := dout;
      end if;
      mem_din <= ram(to_integer(unsigned(mem_addr(14 downto 2))));
    end if;
  end process;

  -- I/O service
  P3 : process(clk) is
  begin
    if clk'event and clk = '1' then     -- rising clock edge
      io_rd_d   <= io_rd;
      io_wr_d   <= io_wr;
      wb_addr_d <= wb_addr;
      dout_d    <= dout;
      if io_wr = '1' or io_rd = '1' then
        io_addr_d <= mem_addr;
      end if;
    end if;
  end process;


  uart_wr  <= '1' when io_wr_d = '1' and io_addr_d = to_unsigned(UART_DATA, 16) else '0';
  uart_rd  <= '1' when io_rd_d = '1' and io_addr_d = to_unsigned(UART_DATA, 16) else '0';
  uart_din <= dout_d(7 downto 0);

  -- Writing to simple registers defigned in the entity
  process (clk) is
  begin  -- process
    if clk'event and clk = '1' then     -- rising clock edge
      if io_wr_d = '1' then
        case to_integer(io_addr_d) is
          when I2C_BUS_SEL => 
            i2c_sel_reg <= dout_d(7 downto 0);
          when OUT0_ADDR =>
            out0_reg <= dout_d;
          when OUT1_ADDR =>
            out1_reg <= dout_d;
          when OUT2_ADDR =>
            out2_reg <= dout_d;
          when OUT3_ADDR =>
            out3_reg <= dout_d;
          when others => null;
        end case;
      end if;
    end if;
  end process;


  i2c_master_top_1 : entity work.i2c_master_top
    generic map (
      ARST_LVL => '0')
    port map (
      wb_clk_i     => wb_clk_o,
      wb_rst_i     => wb_rst_o,
      arst_i       => resetq,
      wb_adr_i     => wb_addr_o(2 downto 0),
      wb_dat_i     => wb_dat_o(7 downto 0),
      wb_dat_o     => wb_dat_i(7 downto 0),
      wb_we_i      => wb_we_o,
      wb_stb_i     => wb_stb_o,
      wb_cyc_i     => wb_cyc_o,
      wb_ack_o     => wb_ack_i,
      wb_inta_o    => open,
      scl_pad_i    => scl_i,
      scl_pad_o    => scl_o,
      scl_padoen_o => scl_oen,
      sda_pad_i    => sda_i,
      sda_pad_o    => sda_o,
      sda_padoen_o => sda_oen);

  -- I2C signals switch

  --scl <= '0' when (scl_pad_o = '0') and (scl_padoen_o = '0') else 'Z';
  --sda <= '0' when (sda_pad_o = '0') and (sda_padoen_o = '0') else 'Z';

  process (i2c_sel, scl, scl_o, sda, sda_o, scl_oen, sda_oen) is
  begin  -- process
    scl_i <= scl(i2c_sel);
    sda_i <= sda(i2c_sel);
    for i in 0 to NUM_I2CS-1 loop
      if i = i2c_sel then
        if (scl_o = '0') and (scl_oen = '0') then
          scl(i) <= '0';
        else
          scl(i) <= 'Z';
        end if;
        if (sda_o = '0') and (sda_oen = '0') then
          sda(i) <= '0';
        else
          sda(i) <= 'Z';
        end if;
      else
        scl(i) <= 'Z';
        sda(i) <= 'Z';
      end if;
    end loop;  -- i    
  end process;

  wb_dat_i(31 downto 8) <= (others => '0');


  jwb_regs <= '1' when mem_addr(15 downto 8) = to_unsigned(JWB_REGS_PAGE, 8) else '0';

  --jwb_data <= '1' when mem_addr(15 downto 8)=to_unsigned(16#61#,8) else '0';
  process (mem_addr) is
  begin  -- process
    if mem_addr(15 downto 8) = to_unsigned(JWB_DATA_PAGE, 8) then
      jwb_data <= '1';
    else
      jwb_data <= '0';
    end if;
  end process;


  sv_mem_addr <= std_logic_vector(mem_addr);
  j1b2wb_1 : j1b2wb
    generic map (
      ADRWIDTH  => 8,
      DATAWIDTH => 32)
    port map (
      J1B_CLK      => clk,
      J1B_ARESETN  => resetq,
      J1B_IO_RD    => io_rd,
      J1B_IO_WR    => io_wr,
      J1B_IO_READY => jwb_ready,
      J1B_IO_ADDR  => sv_mem_addr,
      J1B_DOUT     => dout,
      J1B_DIN      => jwb_dout,
      J1B_WB_DATA  => jwb_data,
      J1B_WB_REGS  => jwb_regs,
      wb_clk_o     => wb_clk_o,
      wb_rst_o     => wb_rst_o,
      wb_addr_o    => wb_addr_o,
      wb_dat_o     => wb_dat_o,
      wb_we_o      => wb_we_o,
      wb_sel_o     => wb_sel_o,
      wb_stb_o     => wb_stb_o,
      wb_cyc_o     => wb_cyc_o,
      wb_dat_i     => wb_dat_i,
      wb_err_i     => wb_err_i,
      wb_ack_i     => wb_ack_i);


  io_din <= x"000000" & uart_dout when io_addr_d = to_unsigned(UART_DATA, 16) else
            (0      => uart_ready, 1 => uart_dav, others => '0') when io_addr_d = to_unsigned(UART_STATUS, 16) else
            clk_frq0                                             when io_addr_d = to_unsigned(FRQ_CNT0, 16) else
            clk_frq1                                             when io_addr_d = to_unsigned(FRQ_CNT1, 16) else
            clk_frq2                                             when io_addr_d = to_unsigned(FRQ_CNT2, 16) else
            out0_reg                                             when io_addr_d = to_unsigned(OUT0_ADDR, 16) else
            out1_reg                                             when io_addr_d = to_unsigned(OUT1_ADDR, 16) else
            out2_reg                                             when io_addr_d = to_unsigned(OUT2_ADDR, 16) else
            out3_reg                                             when io_addr_d = to_unsigned(OUT3_ADDR, 16) else
            inp0                                                 when io_addr_d = to_unsigned(INP0_ADDR, 16) else
            inp1                                                 when io_addr_d = to_unsigned(INP1_ADDR, 16) else
            inp2                                                 when io_addr_d = to_unsigned(INP2_ADDR, 16) else
            inp3                                                 when io_addr_d = to_unsigned(INP3_ADDR, 16) else
            x"000000" & i2c_sel_reg                              when io_addr_d = to_unsigned(I2C_BUS_SEL, 16) else
            jwb_dout                                             when (jwb_regs = '1' or jwb_data = '1') else
            (others => '0');

  io_ready <= jwb_ready when (jwb_regs = '1' or jwb_data = '1') else
              '1';

  -- component instantiation
  DUT : j1
    generic map (
      WIDTH => 32)
    port map (
      clk       => clk,
      resetq    => resetq,
      io_rd     => io_rd,
      io_wr     => io_wr,
      io_ready  => io_ready,
      mem_wr    => mem_wr,
      dout      => dout,
      mem_din   => mem_din,
      io_din    => io_din,
      mem_addr  => mem_addr,
      code_addr => code_addr,
      insn      => insn
      );

  uart_2 : uart
    generic map (
      brg_div => 11)
    port map (
      clk         => clk,
      din         => uart_din,
      dout        => uart_dout,
      rx_rd       => uart_rd,
      tx_wr       => uart_wr,
      nrst        => resetq,
      tx_empty    => uart_ready,
      tx_finished => uart_empty,
      rx_full     => uart_dav,
      fr_err      => open,
      ovr_err     => open,
      tx          => uart_tx,
      rx          => uart_rx);

  -- Frequency meters
  frq_counter_0 : frq_counter
    generic map (
      CNT_TIME   => 20000000,
      CNT_LENGTH => 32)
    port map (
      ref_clk => clk,
      rst_p   => '0',
      frq_in  => clk_0,
      frq_out => clk_frq0);

  frq_counter_1 : frq_counter
    generic map (
      CNT_TIME   => 20000000,
      CNT_LENGTH => 32)
    port map (
      ref_clk => clk,
      rst_p   => '0',
      frq_in  => clk_1,
      frq_out => clk_frq1);

  frq_counter_2 : frq_counter
    generic map (
      CNT_TIME   => 20000000,
      CNT_LENGTH => 32)
    port map (
      ref_clk => clk,
      rst_p   => '0',
      frq_in  => clk_2,
      frq_out => clk_frq2);

end architecture test;


