-- 建议在 1960*1080分辨率下设置 1600*900 非全屏模式，并关闭自动开始
local hwn = FindWindow("UnityWndClass", "BloonsTD6")
FocusWindow(hwn)
Delay(100)
local rect = GetClientRect(hwn)
SetBasePosition(rect[0], rect[1])

local MapOffsetX = 353
local MapOffsetY = 261
local luckyMode = 0 -- 抽取哪个奖品 0 随机，1-4 固定

---判断是否是有奖励的关卡，每个版本都需要进行调整 TODO
---@param x number
---@param y number
---@return boolean
function IsLuckyLevel(x, y)
	-- 有奖励的边框的颜色，未考虑黑框和其他颜色
	local color = GetPixelColor(x, y)
	-- 602E10  502111 5D2C10
	return string.find(color, "6") == 1 or string.find(color, "5") == 1
end

---判断是否进入抽奖页面，每个版本都需要进行调整 TODO
---@return boolean
function IsLuckyScene()
	return IsPixelColor(686,47,"BC310A")
end


-- q 飞镖，z 狙击，c 海盗，x 潜艇, j 刺钉, k 猴村, u 英雄, h 农场, f 炼金, s 超猴, a 法师, t 冰猴, y 胶水, d 忍者, v 飞机, b 直升机, r 图钉, e 大炮, w 回旋镖, Back 出售, l 工程猴, g 德鲁伊

---难度选取
---@param diffIndex number #0-2
---@param diffNum number #0-2
function ChangeDiff(diffIndex, diffNum)
	local DiffOffsetX = 277
	Delay(1000)
    MoveTo(diffIndex * DiffOffsetX + 519, 464)
    Delay(500)
    Click(2)
    Delay(500)
    if diffNum < 1
    then
        -- 选取标准模式
        MoveTo(530, 488)
	elseif diffNum == 1
    then
        -- 选取极难模式
        MoveTo(1070, 616)
	elseif diffNum > 1
    then
        -- 选取点击模式
        MoveTo(1337, 616)
    end
    Delay(1000)
    Click(2)
    Delay(1000)
	while IsPixelColor(56,15, "DA2A00") == false
	do
		Delay(1000)
	end
end

function DiffEasy()
    ChangeDiff(0,0)
end

function DiffDeflation()
    ChangeDiff(1,0)
end

function DiffMedium()
    ChangeDiff(2,0)
end

function DiffHard()
    ChangeDiff(2,0)
end

function DiffImpopable()
    ChangeDiff(2,1)
end

function DiffCHIMPS()
    ChangeDiff(2,2)
end

-- 判断当前是否是失败页面
function GameIsLose()
    return IsPixelColor(727, 312, "001FFF")
end

-- 判断游戏是否结束
function GameIsFinish()
    if IsPixelColor(773, 353, "FFEB00")
    then
        return true
    end
    if IsPixelColor(773, 353, "FFEB00")
    then
        return true
    end
    return false
end

-- 当前是否是游戏结束界面
function GameIsEnd()
    if GameIsLose()
    then
        return true
    end
    if GameIsFinish()
    then
        return true
    end
    return false
end

-- 游戏结束，进入首页重新开始
function GameOver()
	Delay(1000)
    if GameIsLose()
    then
        MoveTo(476, 680)
        Delay(100)
        Click()
        Delay(3000)
        Log("游戏失败了！")
        Main();
    end
    -- 胜利的统计数据
    if IsPixelColor(775, 124, "FFEB00")
    then
        MoveTo(793, 757)
        Delay(100)
        Click()
        Delay(1000)
    end
    -- 胜利的返回首页
    if IsPixelColor(773, 353, "FFEB00")
    then
        MoveTo(582, 704)
        Delay(100)
        Click()
        Delay(1000)
        Log("游戏完成！")
        IsLucky()
        Main()
    end
end

--- 判断是否是抽奖页面
function IsLucky()
    -- local GiftOffsetX = 253
    Delay(3000)
    if IsLuckyScene() == false
    then
        return
    end

	local bgColor = "000000"
	if luckyMode > 0
	then
		-- 104, 93
		local luckyIndex = luckyMode - 1
		MoveTo(1195 + luckyIndex % 2 * 104, 340 + luckyIndex // 2 * 93)
		Click()
		Delay(200)
	end
	MoveTo(800,563)
	Click()
	Delay(2000)
	for i = 422, 1179, 126
	do
		if IsPixelColor(i,452, bgColor) == false
		then
			MoveTo(i,452)
			Delay(100)
			Click()
			Delay(2000)
			Click()
			Delay(1000)
		end
		if IsPixelColor(800,837, bgColor) == false
		then
			break
		end
	end
	-- 确认按钮
	if IsPixelColor(800,837, bgColor) == false
	then
		MoveTo(800,837)
		Click(1)
		Delay(1000)
		Input("Esc") --go home
	end
end

---关闭面板
function ClosePanel()
	Delay(200)
    Input("Esc")
	Delay(300)
end

---地图选取
---@param mapIndex number #0-5
function ChangeMap(mapIndex)
    MoveTo(mapIndex % 3 * MapOffsetX + 443, mapIndex // 3 * MapOffsetY + 217)
    Delay(200)
    Click()
    Delay(1000)
end

---英雄选取
---@param index number 0 - 13
function ChangeHero(index)
    local HeroOffsetX = 126
    local HeroOffsetY = 160
    MoveTo(96,821)
    Click()
    Delay(200)

    MoveTo(80 + index % 3 * HeroOffsetX, 184 + index // 3 * HeroOffsetY)
    Delay(200)
    Click(2)
    MoveTo(926,516)
    Delay(500)
    Click()
    Delay(500)
    Input("Esc")
    Delay(1000)
end
--- 昆西
function HeroQuincy()
    ChangeHero(0)
end
--- 格温
function HeroGwen()
    ChangeHero(1)
end
---琼斯
function HeroJones()
    ChangeHero(2)
end
---奥本
function HerObyn()
    ChangeHero(3)
end

---杰拉尔多
function HeroGeraldo()
    ChangeHero(4)
end

---灵机
function HeroPsi()
    ChangeHero(13)
end
---丘吉尔
function HeroTank()
    ChangeHero(5)
end
---本杰明
function HeroBen()
    ChangeHero(6)
end
---艾泽里
function HeroEzili()
    ChangeHero(7)
end
---帕特雪人
function HeroPat()
    ChangeHero(8)
end
---安多拉
function HeroAdora()
    ChangeHero(9)
end
---海盗布里克尔
function HeroBrickell()
    ChangeHero(10)
end
--- ea 外星人
function HeroETA()
    ChangeHero(11)
end
--- 萨乌达
function HeroSauda()
    ChangeHero(12)
end


---放置猴子
---@param key string # 按键
---@param x number # x
---@param y number # y
function DrogMonkey(key, x, y)
    Delay(300)
    Input(key)
    Delay(500)
    MoveTo(x, y)
    Delay(500)
    Click()
    Delay(200)
end

---卖掉猴子
---@param x number
---@param y number
function SellMonkey(x, y)
	MoveTo(x, y)
    Delay(300)
    Click()
	Delay(500)
	Input("Backspace")
	Delay(200)
end

---升级技能
---@param skill number #技能 1-3
---@param count number #次数
function UpgradeSkill(skill, count)
	if count < 1
	then
		count = 1
	end
    for i = 1, count, 1 do
		if skill < 2
		then
			Input(",")
		elseif skill == 2
		then
			Input(".")
		else
			Input("/")
		end
		Delay(200)
	end
end
---点击并升级
---@param skill number
---@param count number
function OpenUpgradeSkill(skill, count)
	Delay(200)
	Click()
	Delay(400)
	UpgradeSkill(skill, count)
end
---切换攻击目标
---@param count number
function SwitchTarget(count)
	if count < 1
	then
		count = 1
	end
	Delay(100)
	Input("Tab", count)
	Delay(200)
end
---点击并切换攻击目标
---@param count number
function OpenSwitchTarget(count)
	Delay(200)
	Click()
	Delay(400)
	SwitchTarget(count)
end

---升级猴子
---@param skill number #1-3
---@param count number
---@param x number
---@param y number
---@param close boolean
function UpgradeMonkey(x, y, skill, count, close)
    MoveTo(x, y)
    OpenUpgradeSkill(skill, count)
    if close == true
    then
        ClosePanel()
    end
end

---升级技能
---@param x number
---@param y number
---@param skills number[]
---@param close boolean
function UpgradeMonkeySkill(x, y, skills, close)
    MoveTo(x, y)
	Delay(100)
    Click()
    Delay(400)
    for _, skill in pairs(skills)
    do
        UpgradeSkill(skill)
    end
    if close == true
    then
        ClosePanel()
    end
end

---等待一关结束
function WaitGradeEnd()
    while IsPixelColor(1507,845,"FFFFFF")
    do
        Delay(1000)
        if GameIsEnd()
        then
            return
        end
    end
end

---设置移动速度
---@param speed number #0|1
function StartGrade(speed)
    Delay(100)
    if IsPixelColor(1507,845,"FFFFFF") == false
    then
        Input("Space")
        Delay(200)
    end
    if IsPixelColor(1514,832,"62CA01")
    then
        if speed > 0
        then
            Input("Space")
        end
    elseif speed < 1
    then
        Input("Space")
    end
end

--- 跳过多少关
---@param count number
function JumpGrade(count)
    for i = 1, count, 1 
    do
        StartGrade(1)
        Delay(1000)
        WaitGradeEnd()
        if GameIsEnd()
        then
            count = 0
            GameOver()
        end
    end
end

--- 避难所脚本
---@see 1.0
function SanctuaryLevel(debug)
	if debug ~= true
	then
		HeroPsi()
		ChangeMap(0)
		DiffHard()
	end

    DrogMonkey("Q", 210,203)
	OpenSwitchTarget()
	DrogMonkey("Q", 1065,350)
	OpenSwitchTarget()
	DrogMonkey("Q",223,249)
	DrogMonkey("Q",1030,388)

	JumpGrade(2)
	
	DrogMonkey("Q", 700,246)

	JumpGrade(4)
	-- 8
	DrogMonkey("U", 348,456)
	OpenSwitchTarget(3)
	ClosePanel()
	
	JumpGrade(2)
	
	DrogMonkey("Z", 634,778)
	
	JumpGrade(6)
	-- 16
	DrogMonkey("X", 615,147)
	OpenUpgradeSkill(1,2)
	UpgradeSkill(3,1)
	ClosePanel()
	
	JumpGrade(8)
	
	DrogMonkey("K", 692,180)
	OpenUpgradeSkill(3,2)
	ClosePanel()
	
	JumpGrade(6)
	
	UpgradeMonkey(615,147,3,2)
	UpgradeMonkey(700,246,3,2)
	ClosePanel()

	JumpGrade(4)
	
	DrogMonkey("X", 776,153)
	OpenUpgradeSkill(1,2)
	UpgradeSkill(3,2)
	ClosePanel()
	
	JumpGrade(2)
	
	UpgradeMonkey(774,147,3)
	DrogMonkey("F", 755,93)
	OpenUpgradeSkill(1,2)
	ClosePanel()
	
	JumpGrade(2)
	
	UpgradeMonkey(755,93,1)
	UpgradeSkill(3)
	ClosePanel()
	
	JumpGrade(2)
	-- 40
	DrogMonkey("K", 654,845)
	OpenUpgradeSkill(3,2)
	UpgradeMonkey(631,768,2,1)
	UpgradeSkill(3,2)
	ClosePanel()
	
	JumpGrade(4)
	
	UpgradeMonkey(631,768,2,2)
	ClosePanel()
	
	JumpGrade(4)
	
	UpgradeMonkey(631,768,2,1,true)
	
	JumpGrade(6)
	
	UpgradeMonkey(631,768,2)
	SwitchTarget()
	ClosePanel()
	
	JumpGrade(2)
	
	DrogMonkey("F", 576,840)
	OpenUpgradeSkill(1,4)
	ClosePanel()
	
	JumpGrade(4)
	
	DrogMonkey("Z", 370,793)
	OpenUpgradeSkill(2,3)
	UpgradeSkill(3,2)
	UpgradeMonkey(598,856,3)
	UpgradeMonkey(672,825,1,2,true)

	JumpGrade(6)
	-- 66
	DrogMonkey("Z", 760,230)
	OpenUpgradeSkill(1,4)
	UpgradeSkill(2,2)
	ClosePanel()

	JumpGrade(4)
	
	UpgradeMonkey(755,93,1)
	DrogMonkey("F", 645,98)
	OpenUpgradeSkill(1,3)
	UpgradeSkill(3)
	ClosePanel()
	
	JumpGrade(2)
	
	UpgradeMonkey(645,98,1,1,true)
	
	JumpGrade(6)
	
	SellMonkey(615,147)
	DrogMonkey("X", 615,147)
	OpenUpgradeSkill(1,2)
	UpgradeSkill(2,4)
	ClosePanel()
	
	JumpGrade(1)
	
	StartGrade(1)
	Delay(1000)
	Input("4")
	WaitGradeEnd()
	GameOver()
end

--峡谷脚本
---@see 1.0
function RavineLevel(debug)
	if debug ~= true
	then
		HeroPsi()
		ChangeMap(1)
		DiffHard()
	end

    DrogMonkey("Q", 858,165)
	OpenSwitchTarget()
	DrogMonkey("Q", 710,823)
	OpenUpgradeSkill(2)
	SwitchTarget()
	DrogMonkey("Q", 237,560)
	OpenSwitchTarget(3)
	DrogMonkey("Q", 220,607)
	
	JumpGrade(1)
	
	DrogMonkey("Q", 634,693)
	
	JumpGrade(6)
	
	DrogMonkey("U", 585,166)
	OpenSwitchTarget(3)
	ClosePanel()
	
	JumpGrade(3)
	-- 12
	DrogMonkey("Q", 180,413)
	OpenSwitchTarget(3)
	DrogMonkey("D", 580,682)
	
	JumpGrade(2)
	
	UpgradeMonkey(580,682,1,1,true)
	
	JumpGrade(2)
	
	DrogMonkey("V", 203,835)
	OpenSwitchTarget()
	ClosePanel()
	
	JumpGrade(4)
	
	DrogMonkey("Z", 369,30)
	UpgradeMonkey(369, 3,1)
	SwitchTarget(3)
	ClosePanel()
	
	JumpGrade(2)
	
	UpgradeMonkey(580,682,3,2,true)
	
	JumpGrade(2)
	
	UpgradeMonkey(203,835,3,2)
	SwitchTarget(3)
	ClosePanel()
	
	JumpGrade(7)
	
	UpgradeMonkey(203,835,3,1,true)
	
	JumpGrade(3)
	
	UpgradeMonkey(203,835,1,2,true)
	
	JumpGrade(4)
	-- 38
	DrogMonkey("K", 349,713)
	OpenUpgradeSkill(1)
	UpgradeSkill(3,2)
	DrogMonkey("F", 347,782)
	OpenUpgradeSkill(1,2)
	ClosePanel()
	
	JumpGrade(2)
	
	DrogMonkey("J", 500,690)
	OpenUpgradeSkill(1)
	UpgradeSkill(3,2)
	SwitchTarget(3)
	ClosePanel()
	
	StartGrade(1)
	Delay(1000)
	Input("1")
	JumpGrade(3)
	
	UpgradeMonkey(500,690,1,2)
	DrogMonkey("J", 205,664)
	OpenUpgradeSkill(1)
	UpgradeSkill(3,2)
	SwitchTarget(3)
	ClosePanel()
	
	JumpGrade(5)
	
	UpgradeMonkey(205,664,1,2)
	UpgradeMonkey(347,782,1)
	UpgradeSkill(3)
	DrogMonkey("F", 294,806)
	OpenUpgradeSkill(1,3)
	UpgradeSkill(3)
	ClosePanel()
	
	JumpGrade(3)
	
	UpgradeMonkey(522,688,1,1,true)
	
	JumpGrade(4)
	
	UpgradeMonkey(347,782,1)
	UpgradeMonkey(294,806,1,1,true)
	
	JumpGrade(5)
	-- 60
	DrogMonkey("V", 204,760)
	OpenUpgradeSkill(1,4)
	UpgradeSkill(2,2)
	SwitchTarget()
	ClosePanel()
	
	JumpGrade(3)
	
	DrogMonkey("J", 286,660)
	OpenUpgradeSkill(1,3)
	UpgradeSkill(3,2)
	SwitchTarget(3)
	ClosePanel()
	
	StartGrade(1)
	Delay(2300)
	Input("2")
	JumpGrade(1)
	
	DrogMonkey("K", 709,221)
	OpenUpgradeSkill(3,2)
	DrogMonkey("Y", 663,135)
	OpenUpgradeSkill(2)
	UpgradeSkill(3,3)
	ClosePanel()
	
	JumpGrade(6)
	
	UpgradeMonkey(286,660,1,1,true)
	
	JumpGrade(6)
	
	StartGrade(1)
	Delay(1000)
	Input("2")
	JumpGrade(2)
	
	DrogMonkey("X", 515,378)
	OpenUpgradeSkill(2,4)
	ClosePanel()
	
	StartGrade(1)
	Delay(4600)
	Input("2")
	Delay(26100)
	Input("2")
	WaitGradeEnd()
	
	StartGrade(1)
	Delay(1000)
	Input("3")
	WaitGradeEnd()
	GameOver()
end

--水淹山谷脚本
---@see 1.0
function FloodyValleyLevel(debug)
	if debug ~= true
	then
		HeroPsi()
		ChangeMap(2)
		DiffHard()
	end
	DrogMonkey("X",775,133)
	DrogMonkey("X",840,640)
	
	JumpGrade(5)
	
	DrogMonkey("U",544,670)
	OpenSwitchTarget(3)
	ClosePanel()
	
	JumpGrade(10)
	
	UpgradeMonkey(840,640, 1, 2)
	UpgradeSkill(3, 2)
	ClosePanel()
	
	JumpGrade(14)
	-- 31
	DrogMonkey("K",386,633)
	OpenUpgradeSkill(3, 2)
	DrogMonkey("J",560,611)
	OpenUpgradeSkill(1)
	UpgradeSkill(3, 2)
	SwitchTarget()
	ClosePanel()
	
	JumpGrade(4)
	
	UpgradeMonkey(560,611,1)
	UpgradeSkill(3)
	ClosePanel()
	
	JumpGrade(3)
	-- 38
	UpgradeMonkey(560,611,3,1,true)
	
	JumpGrade(2)
	-- 40
	DrogMonkey("F",495,605)
	OpenUpgradeSkill(1, 3)
	UpgradeSkill(2, 2)
	ClosePanel()
	
	JumpGrade(4)
	-- 44
	DrogMonkey("Z",260,661)
	OpenUpgradeSkill(2, 3)
	UpgradeSkill(3, 2)
	ClosePanel()
	
	JumpGrade(5)
	-- 49
	UpgradeMonkey(260,661,2,1,true)
	
	JumpGrade(6)
	-- 55
	UpgradeMonkey(260,661,2, 2)
	SwitchTarget()
	ClosePanel()
	
	JumpGrade(4)
	-- 59
	UpgradeMonkey(386,633,1, 2)
	DrogMonkey("F",217,581)
	OpenUpgradeSkill(1, 4)
	UpgradeSkill(3)
	ClosePanel()
	
	JumpGrade(3)
	
	DrogMonkey("Z",201,628)
	OpenUpgradeSkill(2, 3)
	UpgradeSkill(3, 2)
	ClosePanel()
	
	JumpGrade(5)
	-- 67
	DrogMonkey("Z",211,686)
	OpenUpgradeSkill(1, 4)
	UpgradeSkill(2, 2)
	SwitchTarget(3)
	ClosePanel()
	
	JumpGrade(4)
	-- 71
	DrogMonkey("F",273,584)
	OpenUpgradeSkill(1, 4)
	UpgradeSkill(3)
	ClosePanel()
	
	JumpGrade(8)
	
	UpgradeMonkey(771,133,2,4,true)
	
	-- JumpGrade(1)
	
	StartGrade(1)
	Delay(3000)
	Input("4")
	WaitGradeEnd()
	GameOver()
end

--炼狱级脚本
---@see 1.0
function InfernalLevel(debug)
	if debug ~= true
	then
		HeroQuincy()
		ChangeMap(3)
		DiffHard()
	end

    DrogMonkey("U",701,590)
    DrogMonkey("Q",410,226)

    JumpGrade(8)

    DrogMonkey("Z",1340,581)
	OpenSwitchTarget(3)
	DrogMonkey("X",983,223)
	OpenUpgradeSkill(1,2)
	UpgradeSkill(3)
	ClosePanel()

    JumpGrade(17)
	--27
	UpgradeMonkey(1340,581, 1)
	UpgradeMonkey(983,223, 3)
	DrogMonkey("Z", 1293,557)
	OpenUpgradeSkill(2, 2)
	UpgradeSkill(3, 2)
	ClosePanel()
	
	JumpGrade(8)

	UpgradeMonkey(1340,581,2,2)
	UpgradeMonkey(1293,557, 2,1, true)
	JumpGrade(4)
	--39
	DrogMonkey("B",86,472)
	OpenUpgradeSkill(1, 2)
	UpgradeSkill(2, 2)
	ClosePanel()

	JumpGrade(1)

	UpgradeMonkey(86,472, 2,1, true)

	JumpGrade(9)

	DrogMonkey("K",1320,419)
	OpenUpgradeSkill(1, 2)
	UpgradeSkill(3, 2)
	DrogMonkey("F",1318,494)
	OpenUpgradeSkill(1, 4)
	UpgradeSkill(3)
	UpgradeMonkey(1340,581,1,2,true)

	JumpGrade(14)
	-- 63

	UpgradeMonkey(1340,581, 1)
	UpgradeMonkey(1293,557,2,2)
	SwitchTarget()
	ClosePanel()
	
	JumpGrade(15)

	DrogMonkey("X",394,699)
	OpenUpgradeSkill(2, 4)
	ClosePanel()
	
	JumpGrade(1)
	
	StartGrade(1)
	Delay(1000)
	Input("4")
	WaitGradeEnd()
	GameOver()
end

--血坑脚本
---@see 1.0
function BloodyPuddlesLevel(debug)
	if debug ~= true
	then
		HeroETA()
		ChangeMap(4)
		DiffHard()
	end

    DrogMonkey("Q", 1086,773)
	DrogMonkey("X", 993,164)
	DrogMonkey("X", 532,563)
	
	JumpGrade(1)
	
	DrogMonkey("X", 232,374)
	
	JumpGrade(3)
	
	UpgradeMonkey(532,563,3,1,true)
	
	JumpGrade(2)
	
	UpgradeMonkey(993,164,3,1,true)
	
	JumpGrade(4)
	
	DrogMonkey("U", 700,360)
	
	JumpGrade(4)
	
	UpgradeMonkey(993,164,1,2,true)
	
	JumpGrade(4)
	
	UpgradeMonkey(993,164,3,1,true)
	
	JumpGrade(7)
	
	DrogMonkey("Z", 527,25)
	OpenUpgradeSkill(1)
	SwitchTarget(3)
	DrogMonkey("F", 1060,168)
	OpenUpgradeSkill(1,2)
	SwitchTarget(3)
	ClosePanel()
	
	JumpGrade(5)
	
	UpgradeMonkey(993,164,3)
	UpgradeMonkey(700,360,3,1,true)
	
	JumpGrade(3)
	
	UpgradeMonkey(1060,168,1)
	UpgradeSkill(3)
	ClosePanel()
	
	JumpGrade(4)
	
	UpgradeMonkey(993,164,3,1,true)
	
	StartGrade(1)
	Delay(100)
	Input("1")
	JumpGrade(2)
	-- 42
	DrogMonkey("K", 1015,232)
	OpenUpgradeSkill(3,2)
	DrogMonkey("X", 950,120)
	OpenUpgradeSkill(1,2)
	UpgradeSkill(3,2)
	ClosePanel()
	
	JumpGrade(4)
	
	UpgradeMonkey(950,120,3)
	UpgradeMonkey(1058,162,1,1,true)
	
	JumpGrade(2)
	
	UpgradeMonkey(532,563,2,3)
	UpgradeSkill(3)
	ClosePanel()
	
	JumpGrade(12)
	-- 60
	UpgradeMonkey(993,164,3,1,true)
	
	JumpGrade(3)
	
	UpgradeMonkey(950,120,3,1,true)
	
	StartGrade(1)
	Delay(2000)
	Input("2")
	JumpGrade(4)
	
	DrogMonkey("F", 832,135)
	OpenUpgradeSkill(1,4)
	UpgradeSkill(3)
	DrogMonkey("X", 1020,113)
	OpenUpgradeSkill(1,2)
	UpgradeSkill(3,2)
	ClosePanel()
	
	JumpGrade(5)
	
	UpgradeMonkey(1020,113,3,2,true)
	
	JumpGrade(7)
	
	UpgradeMonkey(532,563,2,1,true)
	
	JumpGrade(1)
	
	StartGrade(1)
	Delay(1000)
	Input("3")
	WaitGradeEnd()
	GameOver();
end

--工坊脚本
---@see 1.0
function WorkshopLevel(debug)
	if debug ~= true
	then
		HeroSauda()
		ChangeMap(5)
		DiffHard()
	end

    DrogMonkey("U", 847,415)
	DrogMonkey("Q", 500,418)
	OpenSwitchTarget(3)
	ClosePanel()
	
	JumpGrade(11)
	
	DrogMonkey("J", 1332,596)
	OpenUpgradeSkill(1)
	UpgradeSkill(3,2)
	SwitchTarget()
	ClosePanel()
	
	JumpGrade(13)
	-- 26
	UpgradeMonkey(1332,596,1)
	UpgradeSkill(3)
	DrogMonkey("F", 752,379)
	OpenUpgradeSkill(1,2)
	SwitchTarget(3)
	ClosePanel()
	
	JumpGrade(11)
	-- 37
	UpgradeMonkey(1332,596,3)
	DrogMonkey("F", 1199,584)
	OpenUpgradeSkill(1,3)
	ClosePanel()
	
	JumpGrade(2)
	
	DrogMonkey("J", 1277,546)
	OpenUpgradeSkill(1,2)
	UpgradeSkill(2,2)
	ClosePanel()
	
	JumpGrade(10)
	-- 49
	UpgradeMonkey(1277,546,1,2)
	UpgradeMonkey(1199,584,1)
	UpgradeSkill(3)
	ClosePanel()
	
	JumpGrade(5)
	
	DrogMonkey("R", 853,363)
	OpenUpgradeSkill(3,4)
	UpgradeSkill(1,2)
	UpgradeMonkey(752,379,1,2)
	UpgradeSkill(3)
	ClosePanel()
	
	JumpGrade(16)
	
	UpgradeMonkey(853,363,3)
	DrogMonkey("K", 1010,326)
	OpenUpgradeSkill(1,3)
	ClosePanel()
	
	JumpGrade(10)
	GameOver()
end


--方院脚本
---@see 1.0
function QuadLevel(debug)
	if debug ~= true
	then
		HeroPsi()
		ChangeMap(0)
		DiffHard()
	end

    DrogMonkey("Q", 695,228)
	DrogMonkey("Q", 695,702)
	DrogMonkey("Q", 335,450)
	DrogMonkey("Q", 1066,450)
	
	JumpGrade(5)
	-- 7
	DrogMonkey("U", 776,245)
	OpenSwitchTarget(3)
	ClosePanel()
	
	JumpGrade(9)
	-- 16
	DrogMonkey("X", 725,360)
	OpenUpgradeSkill(1)
	UpgradeSkill(3,2)
	ClosePanel()
	
	JumpGrade(5)
	
	UpgradeMonkey(725,360,1)
	DrogMonkey("Z", 693,599)
	OpenUpgradeSkill(1)
	SwitchTarget(3)
	ClosePanel()
	
	JumpGrade(10)
	--31
	DrogMonkey("J", 249,650)
	DrogMonkey("A", 464,571)
	DrogMonkey("J", 837,405)
	OpenUpgradeSkill(1)
	ClosePanel()
	
	JumpGrade(6)
	
	UpgradeMonkey(837,405,1)
	UpgradeSkill(2,2)
	ClosePanel()
	UpgradeMonkey(464,571,2,2,true)
	
	JumpGrade(3)
	-- 40
	UpgradeMonkey(837,405,1,1,true)
	UpgradeMonkey(249,650,1,2)
	UpgradeSkill(2)
	ClosePanel()
	
	JumpGrade(2)
	
	UpgradeMonkey(249,650,1)
	UpgradeSkill(2)
	ClosePanel()
	
	JumpGrade(8)
	
	DrogMonkey("K", 850,328)
	OpenUpgradeSkill(3,2)
	DrogMonkey("K", 375,708)
	OpenUpgradeSkill(3,2)
	DrogMonkey("T", 885,211)
	OpenUpgradeSkill(1)
	UpgradeSkill(2)
	DrogMonkey("T", 490,708)
	OpenUpgradeSkill(1)
	UpgradeSkill(2)
	DrogMonkey("F", 883,447)
	OpenUpgradeSkill(1,3)
	SwitchTarget(3)
	DrogMonkey("F", 205,700)
	OpenUpgradeSkill(1,3)
	SwitchTarget(3)
	DrogMonkey("J", 924,370)
	OpenUpgradeSkill(1,3)
	DrogMonkey("J", 293,595)
	
	JumpGrade(2)
	
	UpgradeMonkey(296,580,1,3,true)
	
	JumpGrade(2)
	--54
	UpgradeMonkey(296,580,2,2)
	UpgradeMonkey(924,370,2,2,true)
	
	JumpGrade(1)
	
	UpgradeMonkey(881,211,1,2)
	UpgradeSkill(2)
	ClosePanel()
	UpgradeMonkey(498,708,1)
	UpgradeSkill(2)
	ClosePanel()
	
	JumpGrade(1)
	
	UpgradeMonkey(498,708,1)
	DrogMonkey("T", 926,247)
	OpenUpgradeSkill(2)
	UpgradeSkill(3,2)
	ClosePanel()
	
	JumpGrade(4)
	--60
	UpgradeMonkey(881,211,1)
	UpgradeMonkey(926,247,3)
	DrogMonkey("T", 450,675)
	OpenUpgradeSkill(2)
	UpgradeSkill(3,2)
	UpgradeMonkey(498,708,1,2,true)
	
	JumpGrade(3)
	
	UpgradeMonkey(450,675,3,1,true)
	
	JumpGrade(1)
	
	UpgradeMonkey(375,708,1,2)
	UpgradeMonkey(850,328,1,2,true)
	
	JumpGrade(5)
	-- 69
	DrogMonkey("S", 999,272)
	OpenUpgradeSkill(1)
	DrogMonkey("S", 542,593)
	OpenUpgradeSkill(1)
	ClosePanel()
	
	JumpGrade(7)
	
	UpgradeMonkey(999,272,1)
	UpgradeSkill(3)
	UpgradeMonkey(542,593,1)
	UpgradeSkill(3)
	ClosePanel()
	
	StartGrade(1)
	Delay(1000)
	Input("2")
	JumpGrade(2)
	-- 78
	UpgradeMonkey(999,272,3)
	UpgradeMonkey(542,593,1)
	UpgradeSkill(3)
	ClosePanel()
	
	JumpGrade(1)
	
	UpgradeMonkey(999,272,3)
	UpgradeMonkey(542,593,3,1,true)
	
	JumpGrade(1)
	GameOver()
end

--黑暗城堡脚本
---@see 1.0
function DarkCastleLevel(debug)
	if debug ~= true
	then
		HeroBrickell()
		ChangeMap(1)
		DiffHard()
	end
    
    DrogMonkey("Q", 492,410)
	OpenSwitchTarget(3)
	DrogMonkey("Q", 492,515)
	OpenSwitchTarget(3)
	DrogMonkey("X", 909,360)
	
	JumpGrade(3)
	
	UpgradeMonkey(909,360,1)
	UpgradeSkill(3)
	ClosePanel()
	
	JumpGrade(5)

	DrogMonkey("U", 975,357)
	UpgradeMonkey(909,360,1,1,true)
	
	JumpGrade(11)
	-- 21
	UpgradeMonkey(492,410,3,2)
	UpgradeMonkey(909,360,3)
	DrogMonkey("Z", 846,110)
	OpenUpgradeSkill(1)
	UpgradeSkill(3)
	SwitchTarget(3)
	ClosePanel()
	
	JumpGrade(14)
	
	UpgradeMonkey(909,360,3)
	DrogMonkey("X", 910,303)
	OpenUpgradeSkill(1,2)
	UpgradeSkill(3,3)
	DrogMonkey("F", 850,374)
	OpenUpgradeSkill(1,2)
	SwitchTarget(3)
	ClosePanel()
	
	JumpGrade(4)
	-- 39
	UpgradeMonkey(909,360,3)
	UpgradeMonkey(850,374,1)
	UpgradeSkill(3)
	ClosePanel()
	
	JumpGrade(15)
	
	UpgradeMonkey(909,360,3,1,true)
	
	JumpGrade(4)

	UpgradeMonkey(910,303,3,1,true)
	
	DrogMonkey("Q", 333,225)
	OpenUpgradeSkill(3,2)
	DrogMonkey("Q", 333,700)
	OpenUpgradeSkill(3,2)
	ClosePanel()
	
	StartGrade(1)
	Delay(5000)
	Input("1")
	JumpGrade(4)
	
	DrogMonkey("F", 761,368)
	OpenUpgradeSkill(1,3)
	UpgradeSkill(3)
	SwitchTarget(3)
	ClosePanel()
	
	JumpGrade(13)
	-- 75
	DrogMonkey("K", 819,235)
	OpenUpgradeSkill(1,2)
	UpgradeSkill(3,2)
	DrogMonkey("X", 910,245)
	OpenUpgradeSkill(1,2)
	UpgradeSkill(3,4)
	UpgradeMonkey(850,374,1)
	UpgradeMonkey(761,368,1,1,true)
	
	JumpGrade(5)
	GameOver()
end

--泥泞的水坑脚本
---@see 1.0
function MuddyPuddlesLevel(debug)
	if debug ~= true
	then
		HeroQuincy()
		ChangeMap(2)
		DiffHard()
	end

    DrogMonkey("Q", 965,443)
	DrogMonkey("X", 1050,436)
	DrogMonkey("X", 1050,379)
	
	JumpGrade(2)
	
	DrogMonkey("U", 400,371)
	
	JumpGrade(3)
	
	DrogMonkey("X", 695,531)
	
	JumpGrade(5)
	
	UpgradeMonkey(695,531,1,2)
	UpgradeSkill(3)
	ClosePanel()
	
	JumpGrade(3)
	-- 15
	DrogMonkey("Z", 873,382)
	OpenUpgradeSkill(1)
	SwitchTarget(3)
	ClosePanel()
	
	JumpGrade(11)
	
	DrogMonkey("K", 735,466)
	OpenUpgradeSkill(3,2)
	UpgradeMonkey(698,531,3,1,true)
	
	JumpGrade(6)
	
	UpgradeMonkey(960,434,3,2)
	DrogMonkey("F", 653,441)
	OpenUpgradeSkill(1,2)
	UpgradeMonkey(698,531,3,1,true)
	
	JumpGrade(5)
	
	UpgradeMonkey(698,531,3)
	UpgradeMonkey(653,441,1)
	UpgradeSkill(3)
	ClosePanel()
	
	JumpGrade(2)
	
	DrogMonkey("X", 645,495)
	OpenUpgradeSkill(1,2)
	UpgradeSkill(3,2)
	DrogMonkey("Q", 920,114)
	DrogMonkey("Q", 1035,755)
	DrogMonkey("Q", 202,748)
	
	JumpGrade(4)
	
	UpgradeMonkey(645,495,3)
	UpgradeMonkey(653,441,1,1,true)
	
	JumpGrade(15)
	-- 58
	UpgradeMonkey(698,531,3)
	DrogMonkey("F", 813,576)
	OpenUpgradeSkill(1,3)
	UpgradeSkill(3)
	ClosePanel()
	
	JumpGrade(4)
	
	UpgradeMonkey(735,466,1,2)
	UpgradeMonkey(645,495,3,1,true)
	
	JumpGrade(5)
	
	DrogMonkey("X", 665,580)
	OpenUpgradeSkill(1,2)
	UpgradeSkill(3,4)
	UpgradeMonkey(813,576,1,1,true)
	
	JumpGrade(10)
	
	UpgradeMonkey(1050,436,1,2)
	UpgradeSkill(2,4)
	ClosePanel()
	
	JumpGrade(2)
	
	StartGrade(1)
	Delay(1000)
	Input("3")
    WaitGradeEnd()
end

---哎哟！脚本
---@see 1.0
function OuchLevel(debug)
	if debug ~= true
	then
		HeroBrickell()
		ChangeMap(3)
		DiffHard()
	end

    DrogMonkey("Q", 564,137)
	DrogMonkey("X", 652,450)
	DrogMonkey("X", 817,451)
	
	JumpGrade(3)
	
	DrogMonkey("U", 730,525)
	
	JumpGrade(6)
	
	UpgradeMonkey(652,450,1,2)
	UpgradeSkill(3)
	ClosePanel()
	
	JumpGrade(4)
	
	UpgradeMonkey(652,450,3,1,true)
	
	JumpGrade(5)
	
	UpgradeMonkey(652,450,3,1,true)
	
	JumpGrade(7)
	
	DrogMonkey("F", 820,352)
	OpenUpgradeSkill(1,2)
	ClosePanel()
	
	JumpGrade(2)
	
	StartGrade(1)
	Delay(1500)
	Input("1")
	JumpGrade(3)
	
	DrogMonkey("K", 964,451)
	OpenUpgradeSkill(3,2)
	DrogMonkey("Q", 948,166)
	OpenUpgradeSkill(3,2)
	ClosePanel()
	
	JumpGrade(3)
	
	UpgradeMonkey(817,451,1,2)
	UpgradeSkill(3,2)
	ClosePanel()
	
	JumpGrade(2)
	
	StartGrade(1)
	Delay(3000)
	Input("1")
	JumpGrade(2)
	
	UpgradeMonkey(652,450,3)
	UpgradeMonkey(820,352,1)
	UpgradeSkill(3)
	ClosePanel()
	
	StartGrade(1)
	Input("1")
	JumpGrade(7)
	-- 46
	UpgradeMonkey(817,451,3,2)
	UpgradeMonkey(820,352,1)
	DrogMonkey("Q", 160,451)
	OpenUpgradeSkill(3,2)
	DrogMonkey("Q", 1137,451)
	OpenUpgradeSkill(3,2)
	DrogMonkey("Q", 558,750)
	DrogMonkey("Q", 949,750)
	
	JumpGrade(3)
	
	StartGrade(1)
	Delay(1000)
	Input("1")
	JumpGrade(9)
	--58
	UpgradeMonkey(652,450,3,1,true)
	
	StartGrade(1)
	Delay(4500)
	Input("1")
	JumpGrade(4)
	-- 62
	DrogMonkey("T", 852,271)
	OpenUpgradeSkill(1)
	UpgradeSkill(3)
	DrogMonkey("T", 465,564)
	OpenUpgradeSkill(1)
	UpgradeSkill(3)
	DrogMonkey("F", 838,558)
	OpenUpgradeSkill(1,3)
	UpgradeSkill(3)
	ClosePanel()
	
	JumpGrade(5)
	-- 67
	DrogMonkey("X", 716,452)
	OpenUpgradeSkill(1,4)
	UpgradeSkill(2,2)
	UpgradeMonkey(838,558,1,1,true)
	
	JumpGrade(3)
	
	DrogMonkey("X", 808,505)
	OpenUpgradeSkill(1,2)
	UpgradeSkill(3,4)
	ClosePanel()
	
	JumpGrade(5)
	
	StartGrade(1)
	Input("1")
	JumpGrade(2)
	
	StartGrade(1)
	Delay(4000)
	Input("1")
	Delay(26000)
	Input("1")
	JumpGrade(3)
	GameOver()
end

---通杀新手地图
---@param debug boolean
function CommonLevel(debug)
	if debug ~= true
	then
		HeroPsi()
		ChangeMap(0)
		DiffHard()
	end
	
end

---选择关卡
function ChooseLevel()
    local DotColor = "409FFF"

    local DotX = 630 + 31 * 10
    local DotX2 = DotX + 31
    local DotY = 631
    local ColumnX1 = 592
    local ColumnX2 = ColumnX1 + MapOffsetX
    local ColumnX3 = ColumnX2 + MapOffsetX
    local RowY1 = 229
    local RowY2 = RowY1 + MapOffsetY

    MoveTo(1111, 801)
    Delay(1000)
    local flag = 0
    local loopCount = 0
    while flag == 0
    do
        loopCount = loopCount + 1
        if loopCount > 4
        then
            Log("未找到含奖励的地图")
            break
        end
        Click()
        Delay(1000)
        if IsPixelColor(DotX, DotY, DotColor)
        then
            if IsLuckyLevel(ColumnX1, RowY1)
            then
                SanctuaryLevel()
                flag = 1
            elseif IsLuckyLevel(ColumnX2,RowY1)
            then
                RavineLevel()
                flag = 1
            elseif IsLuckyLevel(ColumnX3,RowY1)
            then
                FloodyValleyLevel()
                flag = 1
            elseif IsLuckyLevel(ColumnX1,RowY2)
            then
                InfernalLevel()
                flag = 1
            elseif IsLuckyLevel(ColumnX2,RowY2)
            then
                BloodyPuddlesLevel()
                flag = 1
            elseif IsLuckyLevel(ColumnX3,RowY2)
            then
                WorkshopLevel()
                flag = 1
            end
        elseif IsPixelColor(DotX2, DotY, DotColor)
        then
            if IsLuckyLevel(ColumnX1, RowY1)
            then
                QuadLevel()
                flag = 1
            elseif IsLuckyLevel(ColumnX2,RowY1)
            then
                DarkCastleLevel()
                flag = 1
            elseif IsLuckyLevel(ColumnX3,RowY1)
            then
                MuddyPuddlesLevel()
                flag = 1
            elseif IsLuckyLevel(ColumnX1,RowY2)
            then
                OuchLevel()
                flag = 1
            end
        end
    end
	GameOver()
end

---主入口
function Main()
    MoveTo(699, 785)
    Delay(200)
    Click()
    Delay(500)
    ChooseLevel()
end

Main()