#!/usr/bin/env python

import sys
from datetime import datetime
import time
import array
import struct
import os

try:
    import serial
except:
    print "This tool needs PySerial, but it was not found"
    sys.exit(1)

class FT900Bootloader:
    def __init__(self, ser):
        ser.setDTR(1)
        ser.setRTS(1)
        ser.setDTR(0)
        ser.setRTS(0)
        self.ser = ser
        self.verbose = False
        self.cumcrc = 0

    def rd1(self):
        """ Return the last incoming character, if any """
        n = self.ser.inWaiting()
        if n:
            r = self.ser.read(n)
            return r[-1]
        else:
            return None

    def waitprompt(self):
        # Might already be at the bootloader prompt
        if self.rd1() == '>':
            return

        # Might be at prompt already, or halted. Send ' '
        self.ser.write(' ')
        self.ser.flush()
        time.sleep(0.001)
        if self.rd1() == '>':
            return

        # Is somewhere else, request manual reset
        print "Please press RESET on target board"
        while True:
            s = self.ser.read(1)
            # print repr(s)
            if s == ">":
                break

    def confirm(self):
        self.ser.write("C")
        return struct.unpack("I", self.ser.read(4))[0]

    def version(self):
        self.ser.write("V")
        return struct.unpack("I", self.ser.read(4))[0]

    def pmcrc32(self, a, sz):
        t0 = time.time()
        self.ser.write("Q" + struct.pack("II", a, sz))
        (r, ) = struct.unpack("I", self.ser.read(4))
        if self.verbose:
            t = time.time() - t0
            self.cumcrc += t
            print 'crc', sz, t, self.cumcrc
        return r

    def flashcrc32(self, a, sz):
        t0 = time.time()
        self.ser.write("G" + struct.pack("II", a, sz))
        (r, ) = struct.unpack("I", self.ser.read(4))
        if self.verbose:
            t = time.time() - t0
            self.cumcrc += t
            print 'crc', sz, t, self.cumcrc
        return r

    def ex(self, ):
        self.ser.write("R")
        self.ser.flush()

    def setspeed(self, s):
        if hasattr(self.ser, 'setBaudrate'):
            self.ser.write("S" + struct.pack("I", s))
            self.ser.flush()
            time.sleep(.001)
            self.ser.setBaudrate(s)
            self.ser.flushInput()
            self.ser.flushOutput()

    def loadprogram(self, program):
        pstr = program.tostring()
        self.ser.write("P" + struct.pack("II", 0, len(pstr)))
        self.ser.write(pstr)

    def flash(self, addr, s):
        self.ser.write('F' + struct.pack("II", addr, len(s)) + s)
        (answer, ) = struct.unpack("I", self.ser.read(4))
        assert answer == 0xf1a54ed

    def hardboot(self, ):
        self.ser.write("H")
        self.ser.flush()

class Bye(Exception):
    pass

def collect_screenshot(dest, ser):
    import Image
    t0 = time.time()
    match = "!screenshot"
    have = "X" * len(match)
    while have != match:
        have = (have + ser.read(1))[-len(match):]
    (w, h) = struct.unpack("II", ser.read(8))
    print '%dx%d image' % (w, h),
    sys.stdout.flush()
    if 0:
        imd = ser.read(4 * w * h)
        im = Image.fromstring("RGBA", (w, h), imd)
    else:
        # print [ord(c) for c in ser.read(20)]
        def getn():
            b = ord(ser.read(1))
            n = b
            while b == 255:
                b = ord(ser.read(1))
                n += b
            # print '  length', n
            return n
                
        imd = ""
        for y in range(h):
            # print 'line', y
            prev = 4 * chr(0)
            d = ""
            while len(d) < 4 * w:
                # print '  have', len(d) / 4
                d += prev * getn()
                d += ser.read(4 * getn())
                prev = d[-4:]
            assert len(d) == 4 * w, 'corrupted screen dump stream'
            imd += d
        im = Image.fromstring("RGBA", (w, h), imd)
    (b,g,r,a) = im.split()
    im = Image.merge("RGBA", (r, g, b, a))
    im.convert("RGB").save(dest)
    took = time.time() - t0
    print 'took %.1fs. Wrote RGB image to %s' % (took, dest)
    ser.write('k')

class TetheredFT900:
    def __init__(self, port):
        ser = serial.Serial(port, 115200, timeout=None, rtscts=0)
        self.ser = ser
        self.searchpath = ['.']
        self.log = open("log", "w")

    def boot(self, bootfile = None):
        ser = self.ser
        speed = 921600
        bl = FT900Bootloader(ser)
        ser.setDTR(1)
        ser.setDTR(0)
        bl.waitprompt()

        time.sleep(.001)
        ser.flushInput()

        if bl.confirm() != 0xf70a0d13:
            print 'CONFIRM command failed'
            sys.exit(1)
        bl.setspeed(speed)

        if bl.confirm() != 0xf70a0d13:
            print 'High-speed CONFIRM command failed'
            sys.exit(1)
        if bootfile is not None:
            program = array.array('I', open(bootfile).read())
            bl.loadprogram(program)
        bl.ex()

        time.sleep(.05)
        while True:
            n = ser.inWaiting()
            if not n:
                break
            ser.read(n)

        ser.write("true tethered !\r\n")
        while ser.read(1) != chr(30):
            pass

    def listen(self):
        print 'listen'
        ser = self.ser
        while 1:
            c = ser.read(max(1, ser.inWaiting()))
            sys.stdout.write(repr(c))
            sys.stdout.flush()

    def command_response(self, cmd):
        ser = self.ser
        # print
        # print 'cmd', repr(cmd)
        ser.write(cmd + '\r')
        r = []
        while True:
            c = ser.read(max(1, ser.inWaiting()))
            # print 'got', repr(c)
            r.append(c.replace(chr(30), ''))
            if chr(30) in c:
                # print 'full reponse', repr("".join(r))
                return "".join(r)

    def interactive_command(self, cmd = None):
        ser = self.ser
        if cmd is not None:
            ser.write(cmd + '\r')
        while True:
            if ser.inWaiting() == 0:
                sys.stdout.flush()
            c = ser.read(max(1, ser.inWaiting()))
            sys.stdout.write(c.replace(chr(30), ''))
            self.log.write(c.replace(chr(30), ''))
            if chr(30) in c:
                return

    def include(self, filename, write = sys.stdout.write):

        for p in self.searchpath:
            try:
                incf = open(p + "/" + filename, "rt")
            except IOError:
                continue
            for l in incf:
                # time.sleep(.001)
                # sys.stdout.write(l)
                if l.endswith('\n'):
                    l = l[:-1]
                print l
                if l == "#bye":
                    raise Bye
                l = l.expandtabs(4)
                rs = l.split()
                if rs and rs[0] == 'include':
                    self.include(rs[1])
                else:
                    r = self.command_response(l)
                    if r.startswith(' '):
                        r = r[1:]
                    if r.endswith(' ok\r\n'):
                        r = r[:-5]
                    if 'error: ' in r:
                        print '--- ERROR --'
                        sys.stdout.write(l + '\n')
                        sys.stdout.write(r)
                        raise Bye
                    else:
                        write(r)
                        # print repr(r)
            return
        print "Cannot find file %s in %r" % (filename, self.searchpath)
        raise Bye

    def shellcmd(self, cmd):
        ser = self.ser
        if cmd.startswith('#include'):
            cmd = cmd.split()
            if len(cmd) != 2:
                print 'Usage: #include <source-file>'
            else:
                try:
                    self.include(cmd[1])
                except Bye:
                    pass
        elif cmd.startswith('#flash'):
            cmd = cmd.split()
            if len(cmd) != 2:
                print 'Usage: #flash <dest-file>'
                ser.write('\r')
            else:
                print 'please wait...'
                dest = cmd[1]
                l = self.command_response('serialize')
                print l[:100]
                print l[-100:]
                d = [int(x, 36) for x in l.split()[:-1]]
                open(dest, "w").write(array.array("i", d).tostring())
                print 4*len(d), 'bytes dumped to', dest
        elif cmd.startswith('#setclock'):
            n = datetime.utcnow()
            cmd = "decimal %d %d %d %d %d %d >time&date" % (n.second, n.minute, n.hour, n.day, n.month, n.year)
            ser.write(cmd + "\r\n")
            ser.readline()
        elif cmd.startswith('#bye'):
            sys.exit(0)
        elif cmd.startswith('#measure'):
            ser = self.ser
            # measure the board's clock
            cmd = ":noname begin $21 emit 100000000 0 do loop again ; execute\r\n"
            time.time() # warmup
            ser.write(cmd)
            while ser.read(1) != '!':
                pass
            t0 = time.time()
            n = 0
            while True:
                ser.read(1)
                t = time.time()
                n += 1
                print "%.6f MHz" % ((2 * 100.000000 * n) / (t - t0))
        elif cmd.startswith('#screenshot'):
            cmd = cmd.split()
            if len(cmd) != 2:
                print 'Usage: #screenshot <dest-image-file>'
                ser.write('\r')
            else:
                dest = cmd[1]
                ser.write('GD.screenshot\r\n')
                collect_screenshot(dest, ser)
                ser.write('\r\n')
        elif cmd.startswith('#movie'):
            cmd = cmd.split()
            if len(cmd) != 2:
                print 'Usage: #movie <command>'
                ser.write('\r')
            else:
                dest = cmd[1]
                ser.write('%s\r' % cmd[1])
                for i in xrange(10000):
                    collect_screenshot("%04d.png" % i, ser)
                ser.write('\r\n')
        else:
            # texlog.write(r"\underline{\textbf{%s}}" % cmd)
            self.interactive_command(cmd)

    def shell(self, autocomplete = True):
        import readline
        import os
        histfile = os.path.join(os.path.expanduser("~"), ".swapforthhist")
        try:
            readline.read_history_file(histfile)
        except IOError:
            pass
        import atexit
        atexit.register(readline.write_history_file, histfile)

        if autocomplete:
            words = sorted((self.command_response('words')).split())
            print 'Loaded', len(words), 'words'
            def completer(text, state):
                text = text.lower()
                candidates = [w for w in words if w.startswith(text)]
                if state < len(candidates):
                    return candidates[state]
                else:
                    return None
            if 'libedit' in readline.__doc__:
                readline.parse_and_bind("bind ^I rl_complete")
            else:
                readline.parse_and_bind("tab: complete")
            readline.set_completer(completer)
            readline.set_completer_delims(' ')

        ser = self.ser
        while True:
            try:
                cmd = raw_input('>').strip()
                self.shellcmd(cmd)
            except KeyboardInterrupt:
                ser.write(chr(3))
                ser.flush()
                self.interactive_command()
            except EOFError:
                # texlog.write(r"\end{Verbatim}" + '\n')
                # texlog.write(r"\end{framed}" + '\n')
                break

if __name__ == '__main__':
    port = '/dev/ttyUSB0'
    image = None

    r = None
    searchpath = []

    args = sys.argv[1:]
    while args:
        a = args[0]
        if a.startswith('-i'):
            image = args[1]
            args = args[2:]
        elif a.startswith('-h'):
            port = args[1]
            args = args[2:]
        elif a.startswith('-p'):
            searchpath.append(args[1])
            args = args[2:]
        else:
            if not r:
                r = TetheredFT900(port)
                r.boot(image)
                r.searchpath += searchpath
            if a.startswith('-e'):
                print r.shellcmd(args[1])
                args = args[2:]
            else:
                r.include(a)
                args = args[1:]
    if not r:
        r = TetheredFT900(port)
        r.boot(image)
        r.searchpath += searchpath
    r.shell()
