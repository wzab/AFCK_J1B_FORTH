-------------------------------------------------------------------------------
--  The code below is a very simple implementation of the UART interface
--  it has been written by Wojciech M. Zabolotny (wzab@ise.pw.edu.pl)
--  on 27.07.2006.
--  This code has been written from scratch, however it was
--  inspired by many different existing UART implementations.
--  Therefore please consider this code to be PUBLIC DOMAIN
--  No warranty of any kind!!!
--
--  The UART has been written specifically to work with the fpgadbg core
--  Therefore it does not use buffering.
-------------------------------------------------------------------------------
library ieee;
use ieee.std_logic_1164.all;
use ieee.numeric_std.all;
use ieee.std_logic_unsigned.all;
library work;

entity uart is
  generic (
    brg_div : integer);
  port (
    clk         : in  std_logic;        -- System clock 
    din         : in  std_logic_vector(7 downto 0);  -- input data
    dout        : out std_logic_vector(7 downto 0);  -- output data
    rx_rd       : in  std_logic;        -- RX read strobe
    tx_wr       : in  std_logic;        -- TX write strobe
    nrst        : in  std_logic;        -- reset signal
    tx_empty    : out std_logic;        -- UART ready to accept next character
    tx_finished : out std_logic;        -- UART has finished sending of data
    rx_full     : out std_logic;        -- There are data in the receiver
    fr_err      : out std_logic;        -- frame error
    ovr_err     : out std_logic;        -- overrun error
    tx          : out std_logic;        -- TX line
    rx          : in  std_logic         -- RX line
    );

end uart;

architecture rtl of uart is

  signal rcv_div                       : integer range 0 to 23;  -- Used to calculate the next sampling
                                        -- moment in the receiver
  signal brg_cnt                       : integer range 0 to brg_div;
  signal tx_div                        : integer range 0 to 15;
  signal send_cntr, rcv_cntr           : integer range 0 to 9;
  signal rx_clk_en, tx_clk_en          : std_logic;
  signal s_rx_full                     : std_logic                    := '0';
  signal s_tx_empty, rx_in, rx_sampled : std_logic                    := '1';
  signal rx_reg, tx_reg                : std_logic_vector(7 downto 0);
  signal smpl                          : std_logic_vector(1 downto 0) := (others => '1');

begin  -- rtl

  tx_empty <= s_tx_empty;
  rx_full  <= s_rx_full;

  -----------------------------------------------------------------------------
  -- Timing generator
  -----------------------------------------------------------------------------
  timing : process (clk, nrst)
  begin  -- process timing
    if nrst = '0' then                  -- asynchronous reset (active low)
      brg_cnt   <= 0;
      tx_div    <= 0;
      rx_clk_en <= '0';
      tx_clk_en <= '0';
    elsif clk'event and clk = '1' then  -- rising clock edge
      rx_clk_en <= '0';
      tx_clk_en <= '0';
      if brg_cnt < brg_div - 1 then
        brg_cnt <= brg_cnt + 1;
      else
        brg_cnt   <= 0;
        rx_clk_en <= '1';
        if tx_div < 15 then
          tx_div <= tx_div+1;
        else
          tx_div    <= 0;
          tx_clk_en <= '1';
        end if;
      end if;
    end if;
  end process timing;


  stx : process (clk, nrst)
  begin  -- process stx
    if nrst = '0' then                  -- asynchronous reset (active low)
      tx         <= '1';
      send_cntr  <= 0;
      s_tx_empty <= '1';
      tx_reg     <= (others => '0');
    elsif clk'event and clk = '1' then  -- rising clock edge
      if tx_wr = '1' then
        tx_reg     <= din;
        s_tx_empty <= '0';
      end if;
      if tx_clk_en = '1' then
        case send_cntr is
          when 0 =>
            if s_tx_empty = '0' then
              tx        <= '0';
              send_cntr <= send_cntr+1;
            end if;
          when 1 to 8 =>
            tx        <= tx_reg(send_cntr-1);
            send_cntr <= send_cntr+1;
          when 9 =>
            tx         <= '1';
            s_tx_empty <= '1';
            send_cntr  <= 0;
          when others => null;
        end case;
      end if;
    end if;
  end process stx;

  -- This process samples the rx line three times and uses the majority voting
  -- to assess the state of the line
  rcv_smpl : process (clk, nrst)
  begin  -- process rcv_smpl
    if nrst = '0' then                  -- asynchronous reset (active low)
      smpl       <= (others => '1');
      rx_sampled <= '1';
    elsif clk'event and clk = '1' then  -- rising clock edge
      if rx_clk_en = '1' then
        smpl(1) <= smpl(0);
        smpl(0) <= rx;
        if (smpl(1) = '1' and smpl(0) = '1') or
          (smpl(1) = '1' and rx = '1') or
          (smpl(0) = '1' and rx = '1') then
          rx_sampled <= '1';
        else
          rx_sampled <= '0';
        end if;
      end if;
    end if;
  end process rcv_smpl;
  -- If you don't want to use the triple sampling, then you can just comment
  -- the next line, and uncomment yet next one. In this case the above
  -- process will be optimized out.
  rx_in <= rx_sampled;
  --rx_in <= rx;

  rcvr : process (clk, nrst)
  begin  -- process
    if nrst = '0' then                  -- asynchronous reset (active low)
      s_rx_full <= '0';
      fr_err    <= '0';
      ovr_err   <= '0';
      rcv_div   <= 0;
      rcv_cntr  <= 0;
    elsif clk'event and clk = '1' then  -- rising clock edge
      if rx_rd = '1' then
        s_rx_full <= '0';
      end if;
      if rx_clk_en = '1' then
        if rcv_div > 0 then
          rcv_div <= rcv_div - 1;
        end if;
        case rcv_cntr is
          when 0 =>
            -- waiting for start bit;
            if rx_in = '0' then
              rcv_div  <= 23;           -- First data bit will be sampled after
                                        -- 24 clocks (in the middle of the next
                                        -- bit)
              rcv_cntr <= 1;
            end if;
          when 1 to 8 =>
            -- receiving data bits;
            if rcv_div = 0 then
              rx_reg(rcv_cntr-1) <= rx_in;
              rcv_div            <= 15;  -- Next data bit will be sampled after
                                         -- 16 clocks
              rcv_cntr           <= rcv_cntr+1;
            end if;
          when 9 =>
            -- checking the stop bit
            if rcv_div = 0 then
              if rx_in = '1' then
                -- stop bit correct
                fr_err    <= '0';
                s_rx_full <= '1';
              else
                -- wrong stop bit -- frame error
                fr_err    <= '1';
                s_rx_full <= '1';
              end if;
              -- check, if the overrun occured
              if s_rx_full = '1' then
                ovr_err <= '1';
              else
                ovr_err <= '0';
              end if;
              dout     <= rx_reg;
              rcv_cntr <= 0;
            end if;
          when others => null;
        end case;
      end if;
    end if;
  end process rcvr;

end rtl;
