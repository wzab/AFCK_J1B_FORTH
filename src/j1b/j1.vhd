-- This file implements the orginal J1 Forth CPU written by James Bowman
-- but translated to VHDL by Wojciech M. Zabolotny
-- see https://github.com/wzab/swapforth for the newest version of the port
-- This file is licensed uder the original J1 license.
-- see https://github.com/jamesbowman/swapforth for original sources.
-- I have retained the original Verilog vode in comments, to allow
-- verification of the translation

library ieee;
use ieee.std_logic_1164.all;
use ieee.numeric_std.all;
use std.textio.all;

entity j1 is
  generic (
    WIDTH : integer := 32);
  port (
    clk       : in  std_logic;
    resetq    : in  std_logic;
    io_rd     : out std_logic;
    io_wr     : out std_logic;
    io_ready  : in  std_logic;
    mem_addr  : out unsigned(15 downto 0)              := (others => '0');
    mem_wr    : out std_logic;
    dout      : out std_logic_vector(WIDTH-1 downto 0) := (others => '0');
    mem_din   : in  std_logic_vector(WIDTH-1 downto 0);
    io_din    : in  std_logic_vector(WIDTH-1 downto 0);
    code_addr : out unsigned(12 downto 0)              := (others => '0');
    insn      : in  std_logic_vector(15 downto 0)
    );
end entity;

architecture rtl of j1 is

  signal dsp, dspN : std_logic_vector(4 downto 0)       := (others => '0');  -- Data stack pointer
  signal st0, st0N : std_logic_vector(WIDTH-1 downto 0) := (others => '0');  -- Top of data stack
  signal dstkW     : std_logic;         -- D stack write

  signal s_io_wr, s_io_rd                                           : std_logic                          := '0';
  signal pc, pcN, pc_plus_1                                         : std_logic_vector(12 downto 0)      := (others => '0');
  signal rstkW                                                      : std_logic                          := '0';  -- R stack write
  signal rstkD                                                      : std_logic_vector(WIDTH-1 downto 0) := (others => '0');  -- R stack write value
  signal reboot                                                     : std_logic                          := '1';
  signal st1, rst0                                                  : std_logic_vector(WIDTH-1 downto 0) := (others => '0');
  signal func_T_N, func_T_R, func_write, func_iow, func_ior, is_alu : std_logic                          := '0';
  signal dspI, rspI                                                 : std_logic_vector(1 downto 0)       := (others => '0');

  signal no_wait : boolean   := true;
  signal clk_ena : std_logic := '0';

  function or_reduce (
    constant vec : std_logic_vector)
    return std_logic is
    variable res : std_logic;
  begin  -- function reduce_or
    res := '0';
    for i in vec'low to vec'high loop
      if vec(i) = '1' then
        res := '1';
      end if;
    end loop;  -- i
    return res;
  end function or_reduce;

  component stack is
    generic (
      WIDTH : integer;
      DEPTH : integer);
    port (
      clk     : in  std_logic;
      clk_ena : in  std_logic;
      rd      : out std_logic_vector(WIDTH-1 downto 0);
      we      : in  std_logic;
      delta   : in  std_logic_vector(1 downto 0);
      wd      : in  std_logic_vector(WIDTH-1 downto 0));
  end component stack;

begin  -- architecture rtl
  pc_plus_1 <= std_logic_vector(unsigned(pc) + 1);
  mem_addr  <= unsigned(st0(15 downto 0));

  -- The D and R stacks
  --stack #(.DEPTH(32)) dstack(.clk(clk), .rd(st1),  .we(dstkW), .wd(st0),   .delta(dspI));
  stack_1 : stack
    generic map (
      WIDTH => WIDTH,
      DEPTH => 32)
    port map (
      clk     => clk,
      clk_ena => clk_ena,
      rd      => st1,
      we      => dstkW,
      delta   => dspI,
      wd      => st0);
  --stack #(.DEPTH(32)) rstack(.clk(clk), .rd(rst0), .we(rstkW), .wd(rstkD), .delta(rspI));
  stack_2 : stack
    generic map (
      WIDTH => WIDTH,
      DEPTH => 32)
    port map (
      clk     => clk,
      clk_ena => clk_ena,
      rd      => rst0,
      we      => rstkW,
      delta   => rspI,
      wd      => rstkD);

  --always @*
  --begin
  process (dsp, insn, io_din, mem_din, rst0,
           st0, st1) is
    variable psel : std_logic_vector(7 downto 0);
  begin  -- process
    -- Compute the new value of st0
    -- casez ({insn[15:8]})
    psel := insn(15 downto 8);
    --8'b1??_?????: st0N = { {(`WIDTH - 15){1'b0}}, insn[14:0] };    // literal
    if std_match(psel, "1-------") then
      st0N              <= (others => '0');
      st0N(14 downto 0) <= insn(14 downto 0);
    --8'b000_?????: st0N = st0;                     // jump
    elsif std_match(psel, "000-----") then
      st0N <= st0;
    --8'b010_?????: st0N = st0;                     // call
    elsif std_match(psel, "010-----") then
      st0N <= st0;
    --8'b001_?????: st0N = st1;                     // conditional jump
    elsif std_match(psel, "001-----") then
      st0N <= st1;
    --8'b011_?0000: st0N = st0;                     // ALU operations...
    elsif std_match(psel, "011-0000") then
      st0N <= st0;
    --8'b011_?0001: st0N = st1;
    elsif std_match(psel, "011-0001") then
      st0N <= st1;
    --8'b011_?0010: st0N = st0 + st1;
    elsif std_match(psel, "011-0010") then
      st0N <= std_logic_vector(unsigned(st0)+unsigned(st1));
    --8'b011_?0011: st0N = st0 & st1; 
    elsif std_match(psel, "011-0011") then
      st0N <= st0 and st1;
    --8'b011_?0100: st0N = st0 | st1;
    elsif std_match(psel, "011-0100") then
      st0N <= st0 or st1;
    --8'b011_?0101: st0N = st0 ^ st1;
    elsif std_match(psel, "011-0101") then
      st0N <= st0 xor st1;
    --8'b011_?0110: st0N = ~st0;
    elsif std_match(psel, "011-0110") then
      st0N <= not st0;
    --8'b011_?0111: st0N = {`WIDTH{(st1 == st0)}};
    elsif std_match(psel, "011-0111") then
      if st1 = st0 then
        st0N <= (others => '1');
      else
        st0N <= (others => '0');
      end if;
    --8'b011_?1000: st0N = {`WIDTH{($signed(st1) < $signed(st0))}};
    elsif std_match(psel, "011-1000") then
      if signed(st1) < signed(st0) then
        st0N <= (others => '1');
      else
        st0N <= (others => '0');
      end if;
    --8'b011_?1001: st0N = st1 >> st0[4:0];
    elsif std_match(psel, "011-1001") then
      st0N <= std_logic_vector(shift_right(unsigned(st1), to_integer(unsigned(st0(4 downto 0)))));
    --8'b011_?1010: st0N = st1 << st0[4:0];
    elsif std_match(psel, "011-1010") then
      st0N <= std_logic_vector(shift_left(unsigned(st1), to_integer(unsigned(st0(4 downto 0)))));
    --8'b011_?1011: st0N = rst0;
    elsif std_match(psel, "011-1011") then
      st0N <= rst0;
    --8'b011_?1100: st0N = mem_din;
    elsif std_match(psel, "011-1100") then
      st0N <= mem_din;
    --8'b011_?1101: st0N = io_din;
    elsif std_match(psel, "011-1101") then
      st0N <= io_din;
    --8'b011_?1110: st0N = {{(`WIDTH - 5){1'b0}}, dsp};
    elsif std_match(psel, "011-1110") then
      st0N             <= (others => '0');
      st0N(4 downto 0) <= dsp;
    --8'b011_?1111: st0N = {`WIDTH{(st1 < st0)}};
    elsif std_match(psel, "011-1111") then
      if unsigned(st1) < unsigned(st0) then
        st0N <= (others => '1');
      else
        st0N <= (others => '0');
      end if;
    --default: st0N = {`WIDTH{1'bx}};
    else
      st0N <= (others => '1');
    end if;
  end process;

  func_T_N   <= '1' when insn(6 downto 4) = "001" else '0';
  func_T_R   <= '1' when insn(6 downto 4) = "010" else '0';
  func_write <= '1' when insn(6 downto 4) = "011" else '0';
  func_iow   <= '1' when insn(6 downto 4) = "100" else '0';
  func_ior   <= '1' when insn(6 downto 4) = "101" else '0';

  is_alu  <= '1' when insn(15 downto 13) = "011" else '0';
  mem_wr  <= (not reboot) and is_alu and func_write;
  dout    <= st1;
  s_io_wr <= (not reboot) and is_alu and func_iow;
  s_io_rd <= (not reboot) and is_alu and func_ior;
  io_wr   <= s_io_wr;
  io_rd   <= s_io_rd;
  --assign rstkD = (insn[13] == 1'b0) ? {{(`WIDTH - 14){1'b0}}, pc_plus_1, 1'b0} : st0;
  process (insn(13), pc_plus_1, st0) is
  begin  -- process
    rstkD <= (others => '0');
    if insn(13) = '0' then
      rstkD(13 downto 1) <= pc_plus_1;
    else
      rstkD <= st0;
    end if;
  end process;

  process (func_T_N, insn(1 downto 0), insn(15 downto 13)) is
    variable psel1 : std_logic_vector(2 downto 0);
  begin  -- process
    --casez ({insn[15:13]})
    psel1 := insn(15 downto 13);
    --3'b1??:   {dstkW, dspI} = {1'b1,      2'b01};
    if std_match(psel1, "1--") then
      dstkW <= '1'; dspI <= "01";
    --3'b001:   {dstkW, dspI} = {1'b0,      2'b11};
    elsif std_match(psel1, "001") then
      dstkW <= '0'; dspI <= "11";
    --3'b011:   {dstkW, dspI} = {func_T_N,  insn[1:0]};
    elsif std_match(psel1, "011") then
      dstkW <= func_T_N; dspI <= insn(1 downto 0);
    --default:  {dstkW, dspI} = {1'b0,      2'b00000};
    else
      dstkW <= '0'; dspI <= "00";
    end if;
  end process;

  dspN <= std_logic_vector(unsigned(dsp) + unsigned(dspI(1) & dspI(1) & dspI(1) & dspI));

  process (func_T_R, insn(15 downto 13), insn(3 downto 2)) is
    variable psel2 : std_logic_vector(2 downto 0);
  begin
    --casez ({insn[15:13]})
    psel2 := insn(15 downto 13);
    --3'b010:   {rstkW, rspI} = {1'b1,      2'b01};
    if std_match(psel2, "010") then
      rstkW <= '1'; rspI <= "01";
    --3'b011:   {rstkW, rspI} = {func_T_R,  insn[3:2]};
    elsif std_match(psel2, "011") then
      rstkW <= func_T_R; rspI <= insn(3 downto 2);
    --default:  {rstkW, rspI} = {1'b0,      2'b00};
    else
      rstkW <= '0'; rspI <= "00";
    end if;
  end process;

  process (insn, pc_plus_1, reboot,
           rst0, st0) is
    variable psel3 : std_logic_vector(5 downto 0);
  begin
    --casez ({reboot, insn[15:13], insn[7], |st0})
    psel3 := reboot & insn(15 downto 13) & insn(7) & or_reduce(st0);
    --6'b1_???_?_?:   pcN = 0;
    if std_match(psel3, "1-----") then
      pcN <= (others => '0');
    --6'b0_000_?_?,      
    --6'b0_010_?_?,
    --6'b0_001_?_0:   pcN = insn[12:0];
    elsif std_match(psel3, "0000--") or std_match(psel3, "0010--") or std_match(psel3, "0001-0") then
      pcN <= insn(12 downto 0);
    --6'b0_011_1_?:   pcN = rst0[13:1];
    elsif std_match(psel3, "00111-") then
      pcN <= rst0(13 downto 1);
    --default:        pcN = pc_plus_1;
    else
      pcN <= pc_plus_1;
    end if;
  end process;


  -- The simplistic implementation of I/O wait needed for more complex I/O
  -- busses. No timeout. Corrupted peripheral may hang the system
  no_wait   <= (io_ready = '1') or ((s_io_wr = '0') and (s_io_rd = '0'));
  clk_ena   <= '1'           when no_wait else '0';
  code_addr <= unsigned(pcN) when no_wait else unsigned(pc);

  process (clk) is
  begin  -- process
    if clk'event and clk = '1' then  -- rising clock edge
      if resetq = '0' then                -- asynchronous reset (active low)
        reboot <= '1';
        pc     <= (others => '0');
        dsp    <= (others => '0');
        st0    <= (others => '0');
      else
        reboot <= '0';
        if no_wait then
          pc  <= pcN;
          dsp <= dspN;
          st0 <= st0N;
        end if;
      end if;
    end if;
  end process;

end architecture rtl;
