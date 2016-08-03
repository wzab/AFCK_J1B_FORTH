-- This file is the translation of the stack2.v file written by James Bowman
-- for his J1 Forth CPU.
-- It has been translated to VHDL by Wojciech M. Zabolotny
-- The file is licensed under the original J1 license.
library ieee;
use ieee.std_logic_1164.all;
use ieee.numeric_std.all;
use std.textio.all;

entity stack is
  generic (
    WIDTH : integer := 32;
    DEPTH : integer
    );
  port (
    clk     : in  std_logic;
    clk_ena : in  std_logic;
    rd      : out std_logic_vector(WIDTH-1 downto 0);
    we      : in  std_logic;
    delta   : in  std_logic_vector(1 downto 0);
    wd      : in  std_logic_vector(WIDTH-1 downto 0)
    );
end entity;

architecture rtl of stack is

  constant BITS : integer := (WIDTH * DEPTH) - 1;

  signal move : std_logic;

  signal head, headN : std_logic_vector(WIDTH-1 downto 0);
  signal tail, tailN : std_logic_vector(BITS downto 0);

begin
  move  <= delta(0);
  headN <= wd                                    when we = '1'       else tail(width-1 downto 0);
  tailN <= x"55aa55aa" & tail(BITS downto WIDTH) when delta(1) = '1' else tail(BITS-WIDTH downto 0) & head;

  process (clk) is
  begin  -- process
    if clk'event and clk = '1' then     -- rising clock edge
      if clk_ena = '1' then
        if (we = '1') or (move = '1') then
          head <= headN;
        end if;
        if (move = '1') then
          tail <= tailN;
        end if;
      end if;
    end if;

  end process;
  rd <= head;
end architecture;

